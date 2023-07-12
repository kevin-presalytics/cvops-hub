using Microsoft.Extensions.Hosting;
using System.Threading.Tasks;
using System.Threading;
using System.Threading.Channels;
using MQTTnet;
using lib.models.mqtt;
using System;
using Serilog;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;

namespace lib.services.mqtt
{
    public class MqttClientWorker : BackgroundService
    {
        IHubMqttClient _mqttClient;
        ILogger _logger;
        IHostApplicationLifetime _appLifetime;
        ChannelReader<MqttPublishMessage> _publishReader;
        ChannelReader<MqttSubscriptionMessage> _subscriptionReader;
        List<MqttSubscriptionMessage> _subscriptions = new List<MqttSubscriptionMessage>();
        Queue<MqttApplicationMessage> _messageQueue = new Queue<MqttApplicationMessage>();
        IServiceProvider _serviceProvider;
        List<ChannelWriter<MqttApplicationMessage>> _messageWriters = new List<ChannelWriter<MqttApplicationMessage>>();
        public MqttClientWorker(
            IHostApplicationLifetime appLifetime,
            ILogger logger,
            IHubMqttClient mqttClient,
            ChannelReader<MqttPublishMessage> publishReader,
            ChannelReader<MqttSubscriptionMessage> subscriptionReader,
            IServiceProvider serviceProvider
        )
        {
            _logger = logger;
            _mqttClient = mqttClient;
            _appLifetime = appLifetime;
            _serviceProvider = serviceProvider;
            _publishReader = publishReader;
            _subscriptionReader = subscriptionReader;
            _subscriptions = new List<MqttSubscriptionMessage>();
            _messageQueue = new Queue<MqttApplicationMessage>(100);
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            DiscoverTopicListeners();
            _appLifetime.ApplicationStarted.Register(OnStarted);
            _appLifetime.ApplicationStarted.Register(() => ListenForPublishMessages(stoppingToken));
            _appLifetime.ApplicationStarted.Register(() => ListenForSubscriptions(stoppingToken));
            _appLifetime.ApplicationStopping.Register(OnStopping);
            return Task.CompletedTask;
        }

        public async void OnStarted()
        {
            // This delay exists to give the MQTT Broker to run a health check on the authentication endpoint
            // Eqmx will set an an alarm, and prevent Client Authentication if it detects the endpoint is not healthy
            // Otherwise, the MQTT Client will fail to connect, set the alarm and entry the retry loop
            _logger.Information("Waiting for application startup to complete before starting MQTT Client...");
            await Task.Delay(5000);
           _logger.Information("Starting MQTT Client...");
            try {
                _mqttClient.OnConnected += async (s, e) => await this.HandleConnected();
                _mqttClient.OnMessage += async (s, e) => await this.HandleMessage(e);
                await _mqttClient.Connect();
            } catch (Exception e) {
                _logger.Fatal(e, "Error starting MQTT Client. Exiting...");
                Environment.Exit(1);
            }
            _logger.Information("MQTT Client started.");
        }

        public async void OnStopping()
        {
            _logger.Information("Stopping MQTT Client...");
            await _mqttClient.Disconnect();
            _logger.Information("MQTT Client stopped.");
        }

        private async Task HandleConnected()
        {
            await Task.WhenAll(
                FlushMessageQueue(),
                FlushSubscriptions()
            );
        }

        private async Task FlushMessageQueue()
        {
            while (_messageQueue.Count > 0)
            {
                var message = _messageQueue.Dequeue();
                await _mqttClient.Publish(message);
            }
            _logger.Debug("Message queue flushed.");
        }

        private async void ListenForPublishMessages(CancellationToken stoppingToken)
        {
            _logger.Debug("Listening for publish messages...");
            try {

                while(await _publishReader.WaitToReadAsync(stoppingToken))
                {
                    while(_publishReader.TryRead(out MqttPublishMessage? message))
                    {
                        if (message != null) {
                            var appMessage = message.Payload.AsApplicationMessage(message.Topic, message.Qos);
                            if (_mqttClient.IsConnected) {
                                await _mqttClient.Publish(appMessage);
                            } else {
                                _messageQueue.Enqueue(appMessage);
                            }
                        }
                    }
                }
            } catch (OperationCanceledException) {
                _logger.Debug("Publish message listener cancelled. Shutting down...");
            } catch (Exception e) {
                _logger.Error(e, "Error listening for publish messages.");
            }
        }

        private async Task FlushSubscriptions()
        {
            foreach (var subscription in _subscriptions)
            {
                await _mqttClient.Subscribe(subscription.Topic);
            }
            _logger.Debug("Subscriptions flushed.");
        }

        private async void ListenForSubscriptions(CancellationToken stoppingToken)
        {
            _logger.Debug("Listening for subscription messages...");
            try {
                while(await _subscriptionReader.WaitToReadAsync(stoppingToken))
                {
                    while(_subscriptionReader.TryRead(out MqttSubscriptionMessage message))
                    {
                        if (message.Unsubscribe)
                        {
                            _subscriptions.RemoveAll(s => s.Topic == message.Topic);
                            if (_mqttClient.IsConnected) {
                                await _mqttClient.Unsubscribe(message.Topic);
                            }
                        } else {
                            if (!_subscriptions.Any(s => s.Topic == message.Topic)) {
                                _subscriptions.Add(message);
                            }
                            if (_mqttClient.IsConnected) {
                                await _mqttClient.Subscribe(message.Topic);
                            }   
                        }
                    }
                }
            } catch (OperationCanceledException) {
                _logger.Debug("Subscription message listener cancelled. Shutting down...");
            } catch (Exception e) {
                _logger.Error(e, "Error listening for subscription messages.");
            }
        }

        private async Task HandleMessage(MqttApplicationMessage message)
        {
            foreach( var writer in _messageWriters)
            {
                await writer.WriteAsync(message).ConfigureAwait(false);
            }
        }

        public void DiscoverTopicListeners()
        {
            _messageWriters = _serviceProvider
                .GetServices<IHostedService>()
                .Where(x => x is IChannelOwner<MqttApplicationMessage> && x != this)
                .Select(x => ((IChannelOwner<MqttApplicationMessage>)x).ChannelWriter)
                .ToList();

            _logger.Debug("Discovered {Count} topic listeners.", _messageWriters.Count);
        }

    }
}
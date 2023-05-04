using Microsoft.Extensions.Hosting;
using System.Threading.Tasks;
using System.Threading;
using System;
using Serilog;
using lib.services.mqtt;
using lib.services.mqtt.queue;
using System.Collections.Generic;

namespace lib.services.mqtt.workers
{
    public class MqttClientWorker : BackgroundService
    {
        IHubMqttClient _mqttClient;
        ILogger _logger;
        IQueueBroker _queueBroker;
        IHostApplicationLifetime _appLifetime;
        public MqttClientWorker(
            IHostApplicationLifetime appLifetime,
            ILogger logger,
            IHubMqttClient mqttClient,
            IQueueBroker queueBroker
        )
        {
            _logger = logger;
            _mqttClient = mqttClient;
            _queueBroker = queueBroker;
            _appLifetime = appLifetime;
            
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _appLifetime.ApplicationStarted.Register(OnStarted);
            _appLifetime.ApplicationStopping.Register(OnStopping);   
            return Task.CompletedTask;
        }

        public async void OnStarted()
        {
           _logger.Information("Starting MQTT Client...");
            try {
                _mqttClient.OnMessage += async (s, e) => {
                    _logger.Debug("Received message on topic {topic}", e.Message.Topic);
                    await _queueBroker.HandleApplicationMessage(e.Message);
                };
                _mqttClient.OnConnected += async (s, e) => await this.HandleConnected();
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
            _logger.Information("MQTT Client connected.");
            foreach (var topic in GetTopics())
            {
                await _mqttClient.Subscribe(topic);
                _logger.Information($"MQTT Client subscribed to {topic}.");
            }
        }

        public virtual string[] GetTopics() {
            return new string[]{};
        }
    }


}
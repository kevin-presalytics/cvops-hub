using System.Threading.Tasks;
using MQTTnet;
using lib.models.mqtt;
using Microsoft.Extensions.Hosting;
using System;
using Serilog;

namespace lib.services.mqtt
{

    public interface IMqttTopicListener 
    {
        string TopicFilter {get;}
        Task HandleMessage(MqttApplicationMessage message);
    }

    public abstract class MqttTopicListener : BackgroundService, IMqttTopicListener
    {
        public abstract string TopicFilter { get;}
        protected IHubMqttClient _mqttClient;
        protected IHostApplicationLifetime _appLifetime;
        protected ILogger _logger;

        public MqttTopicListener(
            IHubMqttClient mqttClient,
            IHostApplicationLifetime appLifetime,
            ILogger logger
        ) {
            _mqttClient = mqttClient;
            _appLifetime = appLifetime;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(System.Threading.CancellationToken stoppingToken)
        {
            _appLifetime.ApplicationStopping.Register(OnStopping);
            _mqttClient.OnMessage += async (s, e) => await RouteMessage(e);
            _mqttClient.OnConnected += async (s, e) => await SubscribeToTopic();
            if (_mqttClient.isConnected) {
                await SubscribeToTopic();
            }
        }

        private void OnStopping() {
            _mqttClient.OnConnected -= async (s, e) => await SubscribeToTopic();
            _mqttClient.OnMessage -= async (s, e) => await RouteMessage(e);
        }

        private async Task SubscribeToTopic()
        {
            try {
                await _mqttClient.Subscribe(TopicFilter);
            } catch (Exception ex ) {
                _logger.Error(ex, "Error subscribing to topic {topic}", TopicFilter);
            }
            
        }

        private async Task RouteMessage(MqttApplicationMessage message)
        {
            MqttTopicFilterCompareResult compareResult = MqttTopicFilterComparer.Compare(message.Topic, TopicFilter);
            if (compareResult == MqttTopicFilterCompareResult.IsMatch) {
                await HandleMessage(message);
            }
        }

        public abstract Task HandleMessage(MqttApplicationMessage message);
    }

    public abstract class MqttTopicListenerWithTypedPayload<T> : MqttTopicListener where T : IMqttPayload
    {
        public MqttTopicListenerWithTypedPayload(IHubMqttClient mqttClient, IHostApplicationLifetime appLifetime, ILogger logger) : base(mqttClient, appLifetime, logger) {}

        public abstract Task HandlePayload(T payload);

        public override async Task HandleMessage(MqttApplicationMessage message)
        {

            T payload = message.AsMqttPayload<T>();
            await HandlePayload(payload);
        }

    }
}
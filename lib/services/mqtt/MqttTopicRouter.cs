using MQTTnet;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using System;

namespace lib.services.mqtt
{
    // This Service enables the auto-discovery Services that subscribe to MQTT Topics (aka "TopicListeners")
    // That implement the IMqttTopicListener interface
    // This service should be implemented as a Singleton
    // IMqttTopicListener services should be registered as Transient  
    public interface IMqttTopicRouter
    {
        // This method should be called at application startup to auto-discover all IMqttTopicListener services
        void DiscoverTopicListeners();

        // This method should be called by the MQTT Client Service when a message is received
        void RouteMessage(MqttApplicationMessage message);

        // A Registry of all IMqttTopicListener services
        Dictionary<string, Type> TopicListeners { get; set;}
    }

    public class MqttTopicRouter : IMqttTopicRouter
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger _logger;
        private bool _isConfigured { get; set;}
        public Dictionary<string, Type> TopicListeners { get; set; } = new Dictionary<string, Type>();
        public MqttTopicRouter(IServiceProvider serviceProvider, ILogger logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
            _isConfigured = false;
        }
        public void DiscoverTopicListeners()
        {
            _logger.Information("Auto-discovering TopicListeners...");
            _serviceProvider.GetServices<IMqttTopicListener>().ToList().ForEach(listener => {
                TopicListeners.Add(listener.TopicFilter, listener.GetType());
            });
            _isConfigured = true;
            _logger.Information($"Auto-discovered {TopicListeners.Count} TopicListeners");
            _logger.Debug($"Auto-discovered TopicListeners: {string.Join(", ", TopicListeners.Select(x => x.Value.ToString()))}");
        }

        public void RouteMessage(MqttApplicationMessage message)
        {
            try {
                if (!_isConfigured)
                {
                    // Will Cause short delay on first message received
                    // TODO: Consider doing on the MQTT Client Service OnConnected event
                    DiscoverTopicListeners();
                }
                bool isMessageHandled = false;
                _logger.Debug($"Routing message for Topic: {message.Topic}");
                foreach (KeyValuePair<string, Type> topicListener in TopicListeners)
                {
                    MqttTopicFilterCompareResult compareResult = MqttTopicFilterComparer.Compare(message.Topic, topicListener.Key);
                    if (compareResult == MqttTopicFilterCompareResult.IsMatch)
                    {
                        if (!isMessageHandled) isMessageHandled = true;
                        Task.Run(async () => {
                            try {
                                IMqttTopicListener listener = (IMqttTopicListener)_serviceProvider.GetRequiredService(topicListener.Value);
                                await listener.HandleMessage(message);
                            } catch (Exception ex) {
                                _logger.Error(ex, $"Error handling message for Topic: {message.Topic}");
                            }
                        });
                    }
                }
                if (!isMessageHandled)
                {
                    _logger.Warning($"No TopicListener found for topic: {message.Topic}");
                } else {
                    _logger.Debug($"Message for Topic: {message.Topic} routed to {TopicListeners.Count} TopicListeners");
                }
            } catch (Exception ex) {
                _logger.Error(ex, $"Error routing message for Topic: {message.Topic}");
            }

        }
    }
}
using System.Threading.Tasks;
using MQTTnet;
using Serilog;
using System;
using lib.models.mqtt;
using lib.services.mqtt;
using lib.services.mqtt.queue;

namespace api.services
{
     public class ApiQueueBroker : IQueueBroker
     {
        ILogger _logger;
        public ApiQueueBroker(ILogger logger)
        {
            _logger = logger;
        }

        public Task HandleApplicationMessage(MqttApplicationMessage message)
        {
            // try {
            //     MqttTopicType topicType = message.GetTopicType();
            //     _logger.Debug("Received message on topic {topic}", message.Topic);
            //     switch (topicType) {
            //         default:
            //             _logger.Warning("Received message on unknown topic type {topicType}", topicType);
            //             break;
            //     }
            // } catch (Exception ex) {
            //     _logger.Error("Error handling message on topic {topic}: {ex}", message.Topic, ex);
            // }
            return Task.CompletedTask;
        }
     }
}
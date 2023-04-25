using MQTTnet;
using Serilog;
using System;
using System.Threading.Tasks;

namespace lib.services.mqtt.queue
{
    public interface IQueueBroker
    {
        Task handleApplicationMessage(MqttApplicationMessage message);
    }

    public class QueueBroker : IQueueBroker
    {
        ILogger _logger;
        public QueueBroker(
            ILogger logger
        )
        {
            _logger = logger;
        }
        public Task handleApplicationMessage(MqttApplicationMessage message)
        {
            MqttTopicType topicType = message.GetTopicType();
            _logger.Debug("Received message on topic {topic}", message.Topic);
            switch (topicType) {
                case MqttTopicType.HubApi:
                    throw new NotImplementedException();
                    //break;
                case MqttTopicType.HubController:
                    throw new NotImplementedException();
                    //break;
                case MqttTopicType.HubWorker:
                    throw new NotImplementedException();
                    //break;
                case MqttTopicType.DeviceData:
                    throw new NotImplementedException();
                    //break;
                case MqttTopicType.DeviceCommand:
                    throw new NotImplementedException();
                    //break;
                case MqttTopicType.DeviceStatus:
                    throw new NotImplementedException();
                    //break;
                default:
                    _logger.Warning("Received message on unknown topic type {topicType}", topicType);
                    throw new Exception("Unknown topic type");
                    //break;
            }
        }
    }
}
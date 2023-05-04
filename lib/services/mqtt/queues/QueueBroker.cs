using MQTTnet;
using Serilog;
using System;
using System.Threading.Tasks;
using lib.models.mqtt;

namespace lib.services.mqtt.queue
{
    public interface IQueueBroker
    {
        Task HandleApplicationMessage(MqttApplicationMessage message);
    }

    public class QueueBroker : IQueueBroker
    {
        ILogger _logger;
        IMqttTopicQueue<UserLoginPayload> _userLoginQueue;
        public QueueBroker(
            ILogger logger,
            IMqttTopicQueue<UserLoginPayload> userLoginQueue
        )
        {
            _logger = logger;
            _userLoginQueue = userLoginQueue;
        }
        public async Task HandleApplicationMessage(MqttApplicationMessage message)
        {
            try {
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
                    case MqttTopicType.UserLogin:
                        await _userLoginQueue.EnqueueAsync(message);
                        break;
                    default:
                        _logger.Warning("Received message on unknown topic type {topicType}", topicType);
                        break;
                }
            } catch (Exception ex) {
                _logger.Error("Error handling message on topic {topic}: {ex}", message.Topic, ex);
            }
        }
    }
}
using System.Threading.Tasks;
using MQTTnet;
using Serilog;
using System;
using lib.models.mqtt;
using lib.services.mqtt;
using lib.services.mqtt.queue;

namespace mqtt_controller.services
{
     public class MqttControllerQueueBroker : IQueueBroker
     {
        ILogger _logger;
        IMqttTopicQueue<UserLoginPayload> _userLoginQueue;
        public MqttControllerQueueBroker(
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
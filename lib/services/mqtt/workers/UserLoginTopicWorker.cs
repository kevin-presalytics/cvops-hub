using lib.services.mqtt.queue;
using lib.models.mqtt;
using lib.services.mqtt;
using System;
using Serilog;
using System.Threading.Tasks;


namespace lib.services.mqtt.workers
{
    public class UserLoginTopicWorker : MqttTopicWorker<UserLoginPayload>
    {
        IHubMqttClient _mqttClient;
        public UserLoginTopicWorker(IMqttTopicQueue<UserLoginPayload> queue, ILogger logger, IHubMqttClient mqttClient) : base(queue, logger)
        {
            _mqttClient = mqttClient;
        }
        protected override async Task HandlePayload(UserLoginPayload payload)
        {
            string topic = MqttTopicManager.GetUserNotificationTopic(payload.UserId);
            UserNotificationPayload userNotificationPayload = new UserNotificationPayload {
                Timestamp = DateTimeOffset.UtcNow,
                UserId = payload.UserId,
                Title = $"Welcome {payload.Username}!",
                Body = $"You have successfully logged in.  Get started by a adding a new device",
            };
            await _mqttClient.Publish(userNotificationPayload.AsApplicationMessage(topic));
        }
    }
}

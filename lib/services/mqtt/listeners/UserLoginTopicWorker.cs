using lib.models.mqtt;
using System;
using Serilog;
using System.Threading.Tasks;


namespace lib.services.mqtt.listeners
{
    public class UserLoginTopicListener : MqttTopicListenerWithTypedPayload<UserLoginPayload>
    {
        IHubMqttClient _mqttClient;
        ILogger _logger;
        public UserLoginTopicListener(ILogger logger, IHubMqttClient mqttClient)
        {
            _mqttClient = mqttClient;
            _logger = logger;
        }

        public override string TopicFilter { get => "user/#/login"; }
        public override async Task HandlePayload(UserLoginPayload payload)
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

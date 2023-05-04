using System;
using System.Text.Json.Serialization;
namespace lib.models.mqtt
{
    public enum UserNotificationLevel
    {
        INFO,
        ERROR,
        WARNING,
        SUCCESS
    }
    public class UserNotificationPayload : MqttPayload
    {

        [JsonPropertyName("userId")]
        public Guid UserId { get; set;} = default!;
        [JsonPropertyName("title")]
        public string Title { get; set;} = default!;
        [JsonPropertyName("body")]
        public string Body { get; set;} = default!;
        [JsonPropertyName("level")]
        public UserNotificationLevel Level { get; set;} = default!;
    }
}
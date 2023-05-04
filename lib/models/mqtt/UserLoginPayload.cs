using System;
using System.Text.Json.Serialization;

namespace lib.models.mqtt
{
    public class UserLoginPayload : MqttPayload
    {

        [JsonPropertyName("username")]
        public string Username { get; set; } = default!;
        [JsonPropertyName("userId")]
        public Guid UserId { get; set;} = default!;
    }
}
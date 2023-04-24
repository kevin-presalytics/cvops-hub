// Example response
// {
//     "result": "allow", // options: "allow" | "deny" | "ignore"
//     "is_superuser": true // options: true | false, default value: false
// }
using System.Text.Json.Serialization;

namespace lib.models.mqtt
{
    public class MqttAuthResponse
    {
        // Any of AuthResultOptions
        [JsonPropertyName("result")]
        public string Result { get; set; } = AuthResultOptions.Deny;

        [JsonPropertyName("is_superuser")]
        public bool IsSuperuser { get; set; } = false;
    }

    public static class AuthResultOptions
    {
        public const string 
            Allow = "allow",
            Deny = "deny",
            Ignore = "ignore";
    }
}
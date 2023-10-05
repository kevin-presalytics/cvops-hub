using System;
using System.Text.Json.Serialization;
// Example configuration
// {
//     type = http
//     enable = true

//     method = post
//     url = "http://127.0.0.1:32333/authz/${peercert}?clientid=${clientid}"
//     body {
//         username = "${username}"
//         topic = "${topic}"
//         action = "${action}"
//     }
//     headers {
//         "Content-Type" = "application/json"
//         "X-Request-Source" = "EMQX"
//     }
// }

namespace lib.models.mqtt
{
    public class EqmxAuthorizeConfiguration
    {
        [JsonPropertyName("type")]
        public string Type { get; set; } = "http";

        [JsonPropertyName("enable")]
        public bool Enable { get; set; } = true;

        [JsonPropertyName("method")]
        public string Method { get; set; } = "post";

        [JsonPropertyName("url")]
        public Uri Url { get; set; } = new Uri("http://localhost:8080");

        [JsonPropertyName("body")]
        public EqmxAuthorizeBody Body { get; set; } = new EqmxAuthorizeBody();

        [JsonPropertyName("headers")]
        public EqmxAuthConfigurationHeaders Headers { get; set; } = new EqmxAuthConfigurationHeaders();
        
        [JsonPropertyName("ssl")]
        public EqmxSslConfiguration Ssl { get; set; } = new EqmxSslConfiguration();
         [JsonPropertyName("pool_size")]
        public long PoolSize { get; set; } = 8;

        [JsonPropertyName("connect_timeout")]
        public string ConnectTimeout { get; set; } = "10s";

        [JsonPropertyName("request_timeout")]
        public string RequestTimeout { get; set; } = "5s";

        [JsonPropertyName("enable_pipelining")]
        public long EnablePipelining { get; set; } = 10;
    }

    public class EqmxAuthorizeBody
    {
        [JsonPropertyName("username")]
        public string Username { get; set; } = "${username}";

        [JsonPropertyName("topic")]
        public string Topic { get; set; } = "${topic}";

        [JsonPropertyName("action")]
        public string Action { get; set; } = "${action}";
        [JsonPropertyName("clientid")]
        public string ClientId { get; set; } = "${clientid}";
    }

    public class EqmxAuthorizeResponse {
        [JsonPropertyName("result")]
        public string Result { get; set; } = AuthResultOptions.Deny;
    }
}

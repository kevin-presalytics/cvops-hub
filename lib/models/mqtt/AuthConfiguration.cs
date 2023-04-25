using System;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

// Default Configuration
// {
//     "method": "post",
//     "url": "http://127.0.0.1:8080",
//     "headers": {
//       "content-type": "application/json"
//     },
//     "body": {
//       "username": "${username}",
//       "password": "${password}"
//     },
//     "pool_size": 8,
//     "connect_timeout": "15s",
//     "request_timeout": "5s",
//     "enable_pipelining": 100,
//     "ssl": {
//       "enable": false,
//       "verify": "verify_peer"
//     },
//     "backend": "http",
//     "mechanism": "password_based"
//   }
namespace lib.models.mqtt
{

    public class EqmxAuthConfiguration
    {
        [JsonPropertyName("method")]
        public string Method { get; set; } = "post";

        [JsonPropertyName("url")]
        public Uri Url { get; set; } = new Uri("http://localhost:8080");

        [JsonPropertyName("headers")]
        public EqmxAuthConfigurationHeaders Headers { get; set; } = new EqmxAuthConfigurationHeaders();

        [JsonPropertyName("body")]
        public EqmxAuthBody Body { get; set; } = new EqmxAuthBody();

        [JsonPropertyName("pool_size")]
        public long PoolSize { get; set; } = 8;

        [JsonPropertyName("connect_timeout")]
        public string ConnectTimeout { get; set; } = "10s";

        [JsonPropertyName("request_timeout")]
        public string RequestTimeout { get; set; } = "5s";

        [JsonPropertyName("enable_pipelining")]
        public long EnablePipelining { get; set; } = 10;

        [JsonPropertyName("ssl")]
        public EqmxSslConfiguration Ssl { get; set; } = new EqmxSslConfiguration();

        [JsonPropertyName("backend")]
        public string Backend { get; set; } = "http";

        [JsonPropertyName("mechanism")]
        public string Mechanism { get; set; } = "password_based";
    }

    public class EqmxAuthBody
    {
        [JsonPropertyName("username")]
        public string Username { get; set; } = "${username}";

        [JsonPropertyName("password")]
        public string Password { get; set; } = "${password}";
    }

    public class EqmxAuthConfigurationHeaders
    {
        [JsonPropertyName("content-type")]
        public string ContentType { get; set; } = "application/json";
    }

    public class EqmxSslConfiguration
    {
        [JsonPropertyName("enable")]
        public bool Enable { get; set; } = false;

        [JsonPropertyName("verify")]
        public string Verify { get; set;} = "verify_peer";
    }
}

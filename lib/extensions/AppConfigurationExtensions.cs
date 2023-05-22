using lib.models.configuration;
using System;

namespace lib.extensions
{
    public static class AppConfigurationExtensions
    {
        public static string GetPostgresqlConnectionString(this AppConfiguration config)
        {
            return $"Host={config.Postgresql.Host};Port={config.Postgresql.Port};Database={config.Postgresql.Database};Username={config.Postgresql.Username};Password={config.Postgresql.Password};SslMode={config.Postgresql.SslMode}";
        }

        public static Uri GetMqttConnectionUrl(this AppConfiguration config)
        {
            string host = config.Domain == "localhost" ? "localhost" : $"mqtt.{config.Domain}";
            if (config.MQTT.useTls)
            {
                return new Uri($"mqtts://{host}:{config.MQTT.SecurePort}");
            }
            return new Uri($"mqtt://{host}:{config.MQTT.Port}");
        }
    }
}
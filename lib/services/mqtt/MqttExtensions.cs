using System.Net.Http.Headers;
using Microsoft.Extensions.DependencyInjection;
using lib.models.configuration;
using lib.services.mqtt;

namespace lib.extensions
{
    public static class MQTTExtensions
    {
        public static void AddMQTTAdmin(this IServiceCollection services, AppConfiguration config)
        {
            services.AddHttpClient(MqttAdminHttpClientName.Get(), client => {
                client.BaseAddress = new System.Uri($"http://{config.MQTT.Uri}:{config.MQTT.AdminPort}/api/v5/");
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic",
                    System.Convert.ToBase64String(
                        System.Text.Encoding.ASCII.GetBytes($"{config.MQTT.AdminUsername}:{config.MQTT.AdminPassword}")
                    )
                );
                // client.DefaultRequestHeaders.Accept.Clear();
                // client.DefaultRequestHeaders.Accept.Add(
                //     new MediaTypeWithQualityHeaderValue("application/json")
                // );
                client.DefaultRequestHeaders.UserAgent.Clear();
                client.DefaultRequestHeaders.Add("User-Agent", "cvops-hub");
            });
            services.AddTransient<IMqttAdmin, EqmxMqttAdmin>();
        }

        public static void AddHubMQTTClient(this IServiceCollection services)
        {
            services.AddSingleton<IHubMQTTClient, HubMQTTClient>();
        }

    }
}
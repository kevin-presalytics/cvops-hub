using System;
using System.Threading.Tasks;
using lib.models.configuration;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Collections.Generic;
using System.Text.Json.Nodes;
using System.IO;
using Microsoft.Extensions.Http;
using lib.models.mqtt;

namespace lib.services.mqtt
{
    public interface IMqttAdmin
    {
        Task Setup();
    }
    public class EqmxMqttAdmin : IMqttAdmin
    {
        HttpClient _httpClient;
        Uri _authUri;
        public EqmxMqttAdmin(IHttpClientFactory clientFactory, AppConfiguration configuration)
        {
            _httpClient = clientFactory.CreateClient(MqttAdminHttpClientName.Get());
            _authUri = new Uri(configuration.MQTT.AuthUrl);
        }

        public async Task Setup()
        {
            if (!(await MQTTAuthIsConfigured()))
            {
                await ConfigureMqttAuth();
            }
        }
        private async Task<bool> MQTTAuthIsConfigured()
        {
            //var authenticators = await _httpClient.GetFromJsonAsync<List<JsonObject>>("/authentication");
            HttpResponseMessage msg = await _httpClient.GetAsync("authentication");
            var authenticators = await msg.Content.ReadFromJsonAsync<List<JsonObject>>();
            if (authenticators == null)
            {
                throw new Exception("Could not retrieve authenticators from MQTT server.");
            }
            #pragma warning disable CS8602
            return authenticators.Where(e => e["backend"].ToString() == "http").Any();
            #pragma warning restore CS8602
        }

        private async Task<bool> ConfigureMqttAuth()
        {
            var authConfiguration = new EqmxAuthConfiguration();
            authConfiguration.Url = _authUri;
            HttpResponseMessage msg = await _httpClient.PostAsJsonAsync("authentication", authConfiguration);
            if (msg.IsSuccessStatusCode)
            {
                return true;
            }
            else
            {
                throw new Exception($"Could not configure MQTT authentication. Status code: {msg.StatusCode}");
            }

        }
        
    }

    public static class MqttAdminHttpClientName
    {
        public static string Get() => "mqtt-admin";
    }
}
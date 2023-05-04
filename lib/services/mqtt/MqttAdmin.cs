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
using Serilog;

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
        ILogger _logger;
        public EqmxMqttAdmin(IHttpClientFactory clientFactory, AppConfiguration configuration, ILogger logger)
        {
            _httpClient = clientFactory.CreateClient(MqttAdminHttpClientName.Get());
            _authUri = new Uri(configuration.MQTT.AuthUrl);
            _logger = logger;
        }

        public async Task Setup()
        {
            try {
                if (!(await MQTTAuthIsConfigured()))
                {
                    _logger.Debug("MQTT authentication is not configured. Configuring...");
                    await ConfigureMqttAuth();
                    _logger.Debug("MQTT authentication configured.");
                }
            } catch (Exception ex) {
                _logger.Error(ex, "Error configuring MQTT authentication.");
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
            bool hasHttpBackend = authenticators.Where(e => e["backend"].ToString() == "http").Any();
            #pragma warning restore CS8602
            _logger.Debug($"Http backend found: {hasHttpBackend}");
            return hasHttpBackend;
           
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
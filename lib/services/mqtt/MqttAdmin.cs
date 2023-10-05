using System;
using System.Threading.Tasks;
using lib.models.configuration;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Collections.Generic;
using System.Text.Json.Nodes;
using Serilog;
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
                if (!(await MQTTAuthenticateIsConfigured()))
                {
                    _logger.Debug("MQTT authentication is not configured. Configuring...");
                    await ConfigureMqttAuthentication();
                    _logger.Debug("MQTT authentication configured.");
                }
                if (!(await MqttAclIsConfigured()))
                {
                    _logger.Debug("MQTT ACL is not configured. Configuring...");
                    await ConfigureMqttAcl();
                    _logger.Debug("MQTT ACL configured.");
                }
            } catch (Exception ex) {
                _logger.Error(ex, "Error configuring MQTT authentication.");
            }
        }
        private async Task<bool> MQTTAuthenticateIsConfigured()
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

        private async Task<bool> ConfigureMqttAuthentication()
        {
            var authenticateConfiguration = new EqmxAuthenticateConfiguration();
            authenticateConfiguration.Url = _authUri;
            _logger.Debug($"Configuration MQTT Server to Authenticate via HTTp at URL: {authenticateConfiguration.Url}");
            HttpResponseMessage msg = await _httpClient.PostAsJsonAsync("authentication", authenticateConfiguration);
            if (!msg.IsSuccessStatusCode)
            {
                throw new Exception($"Could not configure MQTT authentication. Status code: {msg.StatusCode}");
            }

            return true;

        }

        private async Task<bool> MqttAclIsConfigured()
        {
            HttpResponseMessage msg = await _httpClient.GetAsync("authorization/sources");
            var aclsResponse = await msg.Content.ReadFromJsonAsync<JsonObject>();
            if (aclsResponse == null)
            {
                throw new Exception("Could not retrieve ACLs from MQTT server.");
            }
            var sources = aclsResponse["sources"];
            #pragma warning disable CS8602
            bool hasHttpAuthorizer = sources.AsArray().Any(s => s["type"].ToString() == "http");
            #pragma warning restore CS8602
            _logger.Debug($"Http authorized found: {hasHttpAuthorizer}");
            return hasHttpAuthorizer;
        }

        private async Task<bool> ConfigureMqttAcl()
        {
             var authorizeConfiguration = new EqmxAuthorizeConfiguration();
            authorizeConfiguration.Url = new Uri(_authUri.AbsoluteUri + "/acl");
            _logger.Debug($"Configuration MQTT Server to Authenticate via HTTp at URL: {authorizeConfiguration.Url}");
            HttpResponseMessage msg = await _httpClient.PostAsJsonAsync("authorization/sources", authorizeConfiguration);
            if (!msg.IsSuccessStatusCode)
            {
                throw new Exception($"Could not configure MQTT authentication. Status code: {msg.StatusCode}");
            }

            return true;
        }
        
    }

    public static class MqttAdminHttpClientName
    {
        public static string Get() => "mqtt-admin";
    }
}
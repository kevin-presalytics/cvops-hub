using System;
using System.Threading;
using System.Threading.Tasks;
using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Formatter;
using MQTTnet.Protocol;
using Serilog;
using lib.models.configuration;

namespace lib.services.mqtt
{
    public interface IHubMqttClient
    {
        Task Connect();
        Task Disconnect();
        Task PingServer();
        Task Subscribe(string topic);
        Task Unsubscribe(string topic);
        Task Publish(string topic, string payload, MqttQualityOfServiceLevel qos);
        Task Publish(MqttApplicationMessage message);
        event EventHandler OnConnected;
    }
    public class HubMqttClient : IDisposable, IHubMqttClient
    {
        private ILogger _logger;
        private IMqttClient _mqttClient;
        private string _mqttUri;
        private string _username;
        private string _password;
        private int _mqttPort;

        private IMqttTopicRouter _mqttTopicRouter;
        public event EventHandler OnConnected;
        

        #pragma warning disable CS8618
        public HubMqttClient(ILogger logger, AppConfiguration configuration, IMqttTopicRouter mqttTopicRouter)
        {
            _logger = logger;
            _mqttUri = configuration.MQTT.Host;
            _mqttPort = configuration.MQTT.Port;
            _username = configuration.MQTT.AdminUsername;
            _password = configuration.MQTT.AdminPassword;
            var mqttFactory = new MqttFactory();
            _mqttClient = mqttFactory.CreateMqttClient();
            _mqttTopicRouter = mqttTopicRouter;
        }
        #pragma warning restore CS8618

        public async Task Disconnect()
        {
            if (!_mqttClient.IsConnected) {
                _logger.Debug("The MQTT client is not connected.");
            } else {
                // This will send the DISCONNECT packet. Calling _Dispose_ without DisconnectAsync the 
                // connection is closed in a "not clean" way. See MQTT specification for more details.
                await this._mqttClient.DisconnectAsync(new MqttClientDisconnectOptionsBuilder().WithReason(MqttClientDisconnectOptionsReason.NormalDisconnection).Build());

                _logger.Debug("The MQTT client is disconnected.");
            }
        }
        
        public async Task PingServer()
        {
            /*
            * This sample sends a PINGREQ packet to the server and waits for a reply.
            *
            * This is only supported in MQTTv5.0.0+.
            */

            if (!_mqttClient.IsConnected) {
                _logger.Debug("The MQTT client is not connected.");
                await this.Connect();
            }

            await this._mqttClient.PingAsync(CancellationToken.None);

            Console.WriteLine("The MQTT server replied to the ping request.");
        }

        public Task Connect()
        {
            /*
            * This approach uses a custom Task/Thread which will monitor the connection status.
            * This is the recommended way but requires more custom code!
            */

            var mqttClientOptions = new MqttClientOptionsBuilder()
                .WithTcpServer(this._mqttUri, this._mqttPort)
                .WithProtocolVersion(MqttProtocolVersion.V500)
                .WithCredentials(_username, _password)
                .Build();
            
            _mqttClient.ApplicationMessageReceivedAsync += e => {
                _logger.Debug("Application Message Received. Topic: {topic}", e.ApplicationMessage.Topic);
                _mqttTopicRouter.RouteMessage(e.ApplicationMessage);
                return Task.CompletedTask;
            };

            _ = Task.Run(
                async () =>
                {
                    // User proper cancellation and no while(true).
                    _logger.Debug("Connection to MqTT Server at {uri}:{port}", this._mqttUri, this._mqttPort);
                    _logger.Debug("Starting Mqtt Client Reconnect Loop...");
                    while (true)
                    {
                        try
                        {
                            // This code will also do the very first connect! So no call to _ConnectAsync_ is required in the first place.
                            if (!await this._mqttClient.TryPingAsync())
                            {
                                await this._mqttClient.ConnectAsync(mqttClientOptions, CancellationToken.None);

                                // Subscribe to topics when session is clean etc.
                                _logger.Debug("The MQTT client is connected.");
                                OnConnected?.Invoke(this, EventArgs.Empty);
                            }
                        }
                        catch (Exception ex)
                        {
                            _logger.Error(ex, "The MQTT client is disconnected. Retrying connection in 5 seconds..");
                        }
                        finally
                        {
                            // Check the connection state every 5 seconds and perform a reconnect if required.
                            await Task.Delay(TimeSpan.FromSeconds(5));
                        }
                    }
                });
            return Task.CompletedTask;
        }

        public async Task Publish(string topic, string payload, MqttQualityOfServiceLevel qos = MqttQualityOfServiceLevel.AtLeastOnce)
        {
            if (!_mqttClient.IsConnected) {
                _logger.Debug("The MQTT client is not connected.");
                await this.Connect();
            }

            var message = new MqttApplicationMessageBuilder()
                .WithTopic(topic)
                .WithPayload(payload)
                .WithQualityOfServiceLevel(qos)
                .WithRetainFlag()
                .Build();

            await this._mqttClient.PublishAsync(message, CancellationToken.None);
        }

        public async Task Publish(MqttApplicationMessage message)
        {
            if (!_mqttClient.IsConnected) {
                _logger.Debug("The MQTT client is not connected.");
                await this.Connect();
            }

            await this._mqttClient.PublishAsync(message, CancellationToken.None);
        }

        public async Task Subscribe(string topic)
        {
            if (!_mqttClient.IsConnected) {
                _logger.Debug("The MQTT client is not connected.");
                await this.Connect();
            }

            var subscribeOptions = new MqttClientSubscribeOptionsBuilder()
                                    .WithTopicFilter(topic)
                                    .Build();

            await this._mqttClient.SubscribeAsync(subscribeOptions);
        }

        public async Task Unsubscribe(string topic)
        {
            if (!_mqttClient.IsConnected) {
                _logger.Debug("The MQTT client is not connected.");
                await this.Connect();
            }

            await this._mqttClient.UnsubscribeAsync(topic);
        }

        void IDisposable.Dispose()
        {
            if (this._mqttClient.IsConnected) {
                this.Disconnect().GetAwaiter().GetResult();
            }
            this._mqttClient.Dispose();
        }
    }
}
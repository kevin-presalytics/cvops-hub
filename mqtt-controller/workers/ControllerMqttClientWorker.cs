using Microsoft.Extensions.Hosting;
using Serilog;
using lib.services.mqtt;


namespace mqtt_controller.workers
{
    public class ControllerMqttClientWorker : MqttClientWorker
    {
        public ControllerMqttClientWorker(
            IHostApplicationLifetime appLifetime,
            ILogger logger,
            IHubMqttClient mqttClient
        ) : base(appLifetime, logger, mqttClient) {}

        public override string[] GetTopics() 
        {
            return new string[] {
                "$share/g/hub/#",
                "$share/g/device/#",
                "$share/g/user/#",
            };
        }
    }
}
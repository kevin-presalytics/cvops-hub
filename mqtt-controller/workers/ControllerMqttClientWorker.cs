using lib.services.mqtt.workers;
using Microsoft.Extensions.Hosting;
using Serilog;
using lib.services.mqtt;
using lib.services.mqtt.queue;
using System.Collections.Generic;


namespace mqtt_controller.workers
{
    public class ControllerMqttClientWorker : MqttClientWorker
    {
        public ControllerMqttClientWorker(
            IHostApplicationLifetime appLifetime,
            ILogger logger,
            IHubMqttClient mqttClient,
            IQueueBroker queueBroker
        ) : base(appLifetime, logger, mqttClient, queueBroker) {}

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
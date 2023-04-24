using Microsoft.Extensions.Hosting;
using System.Threading.Tasks;
using System.Threading;
using System;
using Serilog;
using lib.services.mqtt;

namespace mqtt_controller.workers
{

    public class MqttAdminSetupWorker : BackgroundService
    {
        IMqttAdmin _mqttAdmin;

        ILogger _logger;
        public MqttAdminSetupWorker(ILogger logger, IMqttAdmin mqttAdmin)
        {
            _logger = logger;
            _mqttAdmin = mqttAdmin;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.Information("Setting up MQTT Admin...");
            await _mqttAdmin.Setup();
            _logger.Information("MQTT Admin setup complete.");
        }
    }
}
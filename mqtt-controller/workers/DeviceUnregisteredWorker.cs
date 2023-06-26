using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Serilog;
using lib.services.mqtt.listeners;
using lib.models.mqtt;
using lib.services;

namespace mqtt_controller.workers
{
    public class DeviceUnregisteredWorker : BackgroundService
    {
        IHostApplicationLifetime _appLifetime;
        ILogger _logger;
        DeviceDataTopicListener _deviceDataTopicListener;
        IDeviceService _deviceService;
        DeviceUnregisteredWorker(
            ILogger logger, 
            IHostApplicationLifetime appLifetime, 
            DeviceDataTopicListener deviceDataTopicListener,
            IDeviceService deviceService
        )
        {
            _logger = logger;
            _appLifetime = appLifetime;
            _deviceDataTopicListener = deviceDataTopicListener;
            _deviceService = deviceService;
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _appLifetime.ApplicationStarted.Register(OnStarted);
            return Task.CompletedTask;
        }

        public void OnStarted()
        {
            // Subscribe to device registered event
            _deviceDataTopicListener.DeviceUnregisteredEvent += async (s, e) => await this.HandleDeviceUnregistered(e);

        }

        public async Task HandleDeviceUnregistered(DeviceUnregisteredData deviceUnregisteredData)
        {

            var device = await _deviceService.GetDevice(deviceUnregisteredData.DeviceId);
            await _deviceService.DeleteDevice(device);
        }
    }
}
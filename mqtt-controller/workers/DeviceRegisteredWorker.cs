using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Serilog;
using lib.services.mqtt.listeners;
using lib.models.mqtt;
using lib.services;

namespace mqtt_controller.workers
{
    public class DeviceRegisteredWorker : BackgroundService
    {
        IHostApplicationLifetime _appLifetime;
        ILogger _logger;
        DeviceDataTopicListener _deviceDataTopicListener;
        IDeviceService _deviceService;
        DeviceRegisteredWorker(
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
            _deviceDataTopicListener.DeviceRegisteredEvent += async (s, e) => await this.HandleDeviceRegistered(e);

        }

        public async Task HandleDeviceRegistered(DeviceRegisteredData deviceRegisteredData)
        {
            var device = await _deviceService.GetDevice(deviceRegisteredData.DeviceId);
            device.Name = deviceRegisteredData.DeviceName;
            device.Description = deviceRegisteredData.DeviceDescription;
            await _deviceService.UpdateDevice(device);
        }
    }
}
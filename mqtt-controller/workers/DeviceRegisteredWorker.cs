using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Serilog;
using lib.services.mqtt.listeners;
using lib.models.db;
using System.Text.Json;
using lib.models.dto;
using lib.services.factories;

namespace mqtt_controller.workers
{
    public class DeviceRegisteredWorker : BackgroundService
    {
        IHostApplicationLifetime _appLifetime;
        ILogger _logger;
        PlatformEventTopicListener _platformEventTopicListener;
        IDeviceServiceFactory _deviceServiceFactory;
        DeviceRegisteredWorker(
            ILogger logger, 
            IHostApplicationLifetime appLifetime, 
            PlatformEventTopicListener platformEventTopicListener,
            IDeviceServiceFactory deviceServiceFactory
        )
        {
            _logger = logger;
            _appLifetime = appLifetime;
            _platformEventTopicListener = platformEventTopicListener;
            _deviceServiceFactory = deviceServiceFactory;
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _appLifetime.ApplicationStarted.Register(OnStarted);
            return Task.CompletedTask;
        }

        public void OnStarted()
        {
            // Subscribe to device registered event
            _platformEventTopicListener.DeviceRegisteredEvent += async (s, e) => await this.HandleDeviceRegistered(e);

        }

        public async Task HandleDeviceRegistered(PlatformEvent platformEvent)
        {
            var deviceRegisteredData = JsonSerializer.Deserialize<DeviceRegisteredData>(platformEvent.EventData);
            if (deviceRegisteredData == null) {
                _logger.Error("Failed to deserialize device registered data");
                return;
            }
            using (var deviceService = _deviceServiceFactory.Create())
            {
                var device = await deviceService.GetDevice(deviceRegisteredData.Id);
                if (deviceRegisteredData.Name != null) device.Name = deviceRegisteredData.Name; 
                if (deviceRegisteredData.Description != null) device.Description =  deviceRegisteredData.Description;
                if (deviceRegisteredData.DeviceInfo != JsonDocument.Parse("{}")) device.DeviceInfo = deviceRegisteredData.DeviceInfo;
                await deviceService.UpdateDevice(device);
            }
        }
    }
}
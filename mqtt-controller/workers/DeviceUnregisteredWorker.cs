using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Serilog;
using lib.services.mqtt.listeners;
using lib.models.db;
using System;
using lib.services.factories;

namespace mqtt_controller.workers
{
    public class DeviceUnregisteredWorker : BackgroundService
    {
        IHostApplicationLifetime _appLifetime;
        ILogger _logger;
        PlatformEventTopicListener _platformEventTopicListener;
        IDeviceServiceFactory _deviceServiceFactory;
        DeviceUnregisteredWorker(
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
            _platformEventTopicListener.DeviceUnregisteredEvent += async (s, e) => await this.HandleDeviceUnregistered(e);

        }

        public async Task HandleDeviceUnregistered(PlatformEvent platformEvent)
        {
            if (platformEvent.DeviceId == null) {
                throw new System.Exception("Device id is null");
            }
            using (var _deviceService = _deviceServiceFactory.Create()) 
            {
                var device = await _deviceService.GetDevice((Guid)platformEvent.DeviceId);
                await _deviceService.DeleteDevice(device);
            }
        }
    }
}
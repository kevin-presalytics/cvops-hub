using System.Threading.Tasks;
using Serilog;
using lib.models.db;
using System.Text.Json;
using lib.models.dto;
using lib.services.factories;
using lib.services;
using System;

namespace mqtt_controller.workers
{
    public class DeviceRegistrationWorker : ChannelListener<PlatformEvent>, IChannelOwner<PlatformEvent>
    {
        IScopedServiceFactory<IDeviceService> _deviceServiceFactory;
        public DeviceRegistrationWorker(
            ILogger logger,
            IScopedServiceFactory<IDeviceService> deviceServiceFactory
        ) : base(logger)
        {
            _deviceServiceFactory = deviceServiceFactory;
        }

        public override async Task HandleMessage(PlatformEvent platformEvent)
        {
            switch (platformEvent.EventType) {
                case PlatformEventTypes.DeviceRegistered:
                    await RegisterDevice(platformEvent);
                    break;
                case PlatformEventTypes.DeviceUnregistered:
                    await UnregisterDevice(platformEvent);
                    break;
                default:
                    break;
            }            
        }

        public async Task RegisterDevice(PlatformEvent platformEvent)
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

        public async Task UnregisterDevice(PlatformEvent platformEvent)
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
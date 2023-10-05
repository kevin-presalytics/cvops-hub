using System.Threading.Tasks;
using Serilog;
using lib.models.db;
using System.Text.Json;
using lib.models.dto;
using lib.services.factories;
using lib.services;
using System;
using System.Threading.Channels;
using lib.models.mqtt;
using dto = lib.models.dto;
using lib.extensions;

namespace mqtt_controller.workers
{
    public class DeviceRegistrationWorker : ChannelListener<PlatformEvent>
    {
        IScopedServiceFactory<IDeviceService> _deviceServiceFactory;
        ChannelWriter<MqttPublishMessage> _publishMessageWriter;

        public DeviceRegistrationWorker(
            ILogger logger,
            IScopedServiceFactory<IDeviceService> deviceServiceFactory,
            ChannelWriter<MqttPublishMessage> publishMessageWriter
        ) : base(logger)
        {
            _deviceServiceFactory = deviceServiceFactory;
            _publishMessageWriter = publishMessageWriter;
        }

        public override async Task HandleMessage(PlatformEvent platformEvent)
        {
            switch (platformEvent.EventType) {
                case PlatformEventTypes.DeviceRegistered:
                    await UpdateDevice(platformEvent);
                    break;
                case PlatformEventTypes.DeviceUnregistered:
                    await UnregisterDevice(platformEvent);
                    break;
                case PlatformEventTypes.DeviceUpdated:
                    await UpdateDevice(platformEvent);
                    break;
                case PlatformEventTypes.DeviceDetailsRequest:
                    await SendDeviceDetails(platformEvent);
                    break;
                default:
                    break;
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

        public async Task UpdateDevice(PlatformEvent platformEvent)
        {
            dto.Device? deviceDto = JsonSerializer.Deserialize<dto.Device>(platformEvent.EventData, LocalJsonOptions.DefaultOptions);
            if (deviceDto != null) {
                using (var _deviceService = _deviceServiceFactory.Create()) 
                {
                    var device = await _deviceService.GetDevice(deviceDto.Id);
                    device.Name = deviceDto.Name ?? device.Name;
                    device.Description = deviceDto.Description ?? device.Description;
                    device.DeviceInfo = deviceDto.DeviceInfo;
                    device.ActivationStatus = deviceDto.ActivationStatus;
                    await _deviceService.UpdateDevice(device);
                }
            }
        }

        public async Task SendDeviceDetails(PlatformEvent platformEvent)
        {
            if (platformEvent.DeviceId == null) {
                throw new System.Exception("Device id is null");
            }
            using (var deviceService = _deviceServiceFactory.Create())
            {
                var device = await deviceService.GetDevice((Guid)platformEvent.DeviceId);
                var payload = new PlatformEvent() {
                    EventType = PlatformEventTypes.DeviceDetailsResponse,
                    EventData = JsonSerializer.SerializeToDocument(device.ToDto(), LocalJsonOptions.DefaultOptions),
                    DeviceId = device.Id,
                    WorkspaceId = device.WorkspaceId,
                    ResponseTopic = platformEvent.ResponseTopic
                };
                var publishMessage = new MqttPublishMessage() {
                    Topic = platformEvent.ResponseTopic ?? $"events/device/{device.Id}",
                    Payload = payload
                };
                await _publishMessageWriter.WriteAsync(publishMessage);
            }
        }
    }
}
using lib.models.mqtt;
using System.Threading.Tasks;
using lib.models.db;
using Serilog;
using System;

namespace lib.services.mqtt.listeners
{
    public class DeviceDataTopicListener : MqttTopicListenerWithTypedPayload<MqttDataMessage<IDeviceData>>
    {
        private ITimeSeriesDBService _timeSeriesDBService;
        private IDeviceService _deviceService;
        private ILogger _logger;
        public DeviceDataTopicListener(ITimeSeriesDBService timeSeriesDBService, IDeviceService deviceService, ILogger logger) : base()
        {
            _timeSeriesDBService = timeSeriesDBService;
            _deviceService = deviceService;
            _logger = logger;
        }
        public override string TopicFilter { get => "device/+/data"; }

        public event EventHandler<DeviceRegisteredData>? DeviceRegisteredEvent;
        public event EventHandler<DeviceUnregisteredData>? DeviceUnregisteredEvent;
        public event EventHandler<InferenceResultData>? InferenceResultEvent;

        public override async Task HandlePayload(MqttDataMessage<IDeviceData> payload)
        {
            Device device = await _deviceService.GetDevice(payload.Data.DeviceId);
            string bucketName = GetTimeSeriesDBBucketName(payload.DataType);
            _timeSeriesDBService.Write(payload.Data, bucketName, device.WorkspaceId);
            switch (payload.DataType) {
                case MqttDataMessageType.DeviceRegistered:
                    DeviceRegisteredEvent?.Invoke(this, (DeviceRegisteredData)payload.Data);
                    break;
                case MqttDataMessageType.DeviceUnregistered:
                    DeviceUnregisteredEvent?.Invoke(this, (DeviceUnregisteredData)payload.Data);
                    break;
                case MqttDataMessageType.InferenceResult:
                    InferenceResultEvent?.Invoke(this, (InferenceResultData)payload.Data);
                    break;
                default:
                    _logger.Warning("DeviceDataTopicListener received unknown data type: {DataType}", payload.DataType);
                    break;
            }
        }

        private string GetTimeSeriesDBBucketName(MqttDataMessageType dataType)
        {
            if (dataType == MqttDataMessageType.InferenceResult) {
                return "inference";
            } else {
                return "events";
            }
        }
    }
}
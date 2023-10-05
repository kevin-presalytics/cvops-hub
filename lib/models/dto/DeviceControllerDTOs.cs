// DTO for creating a new Device
// Data used to generate QR code client-side
using System;
using System.Text.Json;
using lib.models.mqtt;
using db = lib.models.db;
using System.Text.Json.Serialization;
using lib.services.mqtt;

namespace lib.models.dto
{
    public class Device : BaseEntity, IMqttPayload
    {
        public DateTimeOffset Time { get; set;} = DateTimeOffset.UtcNow;
        public string? Name { get; set; } = default!;
        public string? Description { get; set; } =  default!;
        public JsonDocument DeviceInfo { get; set; } = JsonDocument.Parse("{}");
        public Guid WorkspaceId { get; set; } = default!;
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public db.DeviceActivationStatus ActivationStatus { get; set; } = db.DeviceActivationStatus.Inactive;
    }
    public class NewDevice
    {
        public Guid Id {get; set;} = default!;
        public string SecretKey {get; set;} = default!;
        public Uri MqttUri {get; set;} = default!;
    }

    public class UpdateDeviceBody {
        public Guid Id { get; set; } = default!;
        public string? Name { get; set; } = default!;
        public string? Description { get; set; } = default!;
    }

    public class DeviceDataTopicMessage : MqttDto
    {

        [JsonConverter(typeof(JsonStringEnumConverter))]
        public DeviceDataTopicTypes Type { get; set; } = default!;
    }

    public class DeviceCommandTopicMessage : MqttDto
    {
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public DeviceCommandTopicTypes Type { get; set; } = default!;
    }

    public enum DeviceDataTopicTypes
 
    {
        DetailsRequest,
    }

    public enum DeviceCommandTopicTypes
    {
        DetailsResponse
    }
}
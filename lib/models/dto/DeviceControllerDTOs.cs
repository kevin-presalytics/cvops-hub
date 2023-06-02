// DTO for creating a new Device
// Data used to generate QR code client-side
using System;
using System.Text.Json;

namespace lib.models.dto
{
    public class Device : BaseEntity
    {
        public string? Name { get; set; } = default!;
        public string? Description { get; set; } =  default!;
        public JsonDocument DeviceInfo { get; set; } = JsonDocument.Parse("{}");
        public Guid WorkspaceId { get; set; } = default!;
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
}
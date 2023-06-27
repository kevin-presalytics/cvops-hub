using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace lib.models.dto
{
    public class DeviceRegisteredData
    {
        [JsonPropertyName("deviceName")]
        public string? Name { get; set; } = default!;
        [JsonPropertyName("deviceDescription")]
        public string? Description { get; set; } = default!;
        [JsonPropertyName("deviceId")]
        public Guid Id { get; set; } = default!;
        public JsonDocument DeviceInfo { get; set; } = JsonDocument.Parse("{}");
    }
}
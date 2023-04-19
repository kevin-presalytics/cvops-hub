// DTO for creating a new Device
// Data used to generate QR code client-side
using System;

namespace lib.models.dto
{
    public class NewDevice
    {
        public Guid Id {get; set;} = default!;
        public string SecretKey {get; set;} = default!;
        public Uri MqttUri {get; set;} = default!;
    }
}
using System;

namespace lib.models.configuration
{
    public class HubConfiguration
    {
        public Guid Id { get; set; } = default!;
        public string Key { get; set; } = default!;
        public Api Api { get; set; } = default!;
        public MqttControllerConfiguration MqttController { get; set; } = default!;
    }
}
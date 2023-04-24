namespace lib.models.configuration
{
    public class HubConfiguration
    {
        public Api Api { get; set; } = default!;
        public MqttControllerConfiguration MqttController { get; set; } = default!;
    }
}
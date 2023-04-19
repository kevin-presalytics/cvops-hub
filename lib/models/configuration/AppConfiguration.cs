// Base configuration type

namespace lib.models.configuration
{
    public class AppConfiguration
    {
        public MQTT MQTT { get; set; } = default!;
        public Postgresql Postgresql { get; set; } = default!;
    }
}
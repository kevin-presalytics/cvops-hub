// Base configuration type

namespace lib.models.configuration
{
    public class AppConfiguration
    {
        public MQTT MQTT { get; set; } = default!;
        public Postgresql Postgresql { get; set; } = default!;
        public Logging Logging { get; set; } = default!;
        public HubConfiguration Hub { get; set; } = default!;
        public Auth Auth { get; set; } = default!;
        public string Domain { get; set; } = default!;
        public TimeSeriesDb TimeSeriesDB { get; set; } = default!;
    }
}
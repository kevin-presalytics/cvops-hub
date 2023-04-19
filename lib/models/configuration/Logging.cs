// Logging configuration
using Serilog.Events;

namespace lib.models.configuration
{
    public class Logging
    {
        // Can be "debug", "info", "error"
        public LogEventLevel Level { get; set; } = default!;
        // can be "text or "json"
        public string Format { get; set; } = default!;
    }
}
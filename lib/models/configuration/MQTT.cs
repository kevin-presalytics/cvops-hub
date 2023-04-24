// MQTT Configuration values
using System;

namespace lib.models.configuration
{
    public class MQTT
    {
        public string Uri { get; set;} = default!;
        public int Port { get; set;} = default!;
        public string AdminUsername { get; set;} = default!;
        public string AdminPassword { get; set;} = default!;
        public string AdminPort { get; set;} = default!;
        public string AuthUrl { get; set;} = default!;

    }
}
// MQTT Configuration values
using System;

namespace lib.models.configuration
{
    public class MQTT
    {
        public string Host { get; set;} = default!;
        public int Port { get; set;} = default!;
        public string AdminUsername { get; set;} = default!;
        public string AdminPassword { get; set;} = default!;
        public string AdminPort { get; set;} = default!;
        public string AuthUrl { get; set;} = default!;
        public int SecurePort { get; set;} = default!;
        public bool useTls { get; set;} = default!;
        public string websocketPort { get; set;} = default!;
        public string websocketProtocol { get; set;} = default!;
        public string Protocol { get; set;} = default!;

    }
}
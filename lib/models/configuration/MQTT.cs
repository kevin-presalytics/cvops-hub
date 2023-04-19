// MQTT Configuration values
using System;

namespace lib.models.configuration
{
    public class MQTT
    {
        public string Uri { get; set;} = default!;

        public int Port { get; set;} = default!;
    }
}
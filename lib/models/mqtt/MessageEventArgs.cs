using MQTTnet;
using System;

namespace lib.models.mqtt
{
    public class MessageEventArgs : EventArgs
    {
        public MqttApplicationMessage Message { get; set; } = default!;

        public MessageEventArgs(MqttApplicationMessage message)
        {
            Message = message;
        }
    }
}
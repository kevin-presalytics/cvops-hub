using MQTTnet;
using Serilog;
using System;
using System.Threading.Tasks;
using lib.models.mqtt;

namespace lib.services.mqtt.queue
{
    public interface IQueueBroker
    {
        Task HandleApplicationMessage(MqttApplicationMessage message);
    }
}
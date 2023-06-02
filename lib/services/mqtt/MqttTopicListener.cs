using System.Threading.Tasks;
using MQTTnet;
using lib.models.mqtt;

namespace lib.services.mqtt
{

    public interface IMqttTopicListener 
    {
        string TopicFilter {get;}
        Task HandleMessage(MqttApplicationMessage message);
    }

    public abstract class MqttTopicListenerWithTypedPayload<T> : IMqttTopicListener where T : IMqttPayload
    {
        public abstract string TopicFilter { get;}

        public abstract Task HandlePayload(T payload);

        public async Task HandleMessage(MqttApplicationMessage message)
        {
            T payload = message.AsMqttPayload<T>();
            await HandlePayload(payload);
        }

    }
}
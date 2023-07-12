using MQTTnet.Protocol;

namespace lib.models.mqtt
{
    public class MqttPublishMessage
    {
        public string Topic {get; set;} = default!;
        public IMqttPayload Payload {get; set;} = default!;
        public MqttQualityOfServiceLevel Qos {get; set;} = MqttQualityOfServiceLevel.AtLeastOnce;
        public bool Retain {get; set;} = false;
    }
}
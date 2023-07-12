namespace lib.models.mqtt
{
    public struct MqttSubscriptionMessage
    {
        public MqttSubscriptionMessage(string topic, bool unsubscribe = false)
        {
            Topic = topic;
            Unsubscribe = unsubscribe;
        }
        public readonly string Topic;
        public readonly bool Unsubscribe;
    }
}
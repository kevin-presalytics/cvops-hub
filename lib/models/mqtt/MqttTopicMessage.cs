using System;
using MQTTnet;
using MQTTnet.Protocol;
using System.Text.Json;
using System.Text.Json.Serialization;
using lib.extensions;

namespace lib.models.mqtt
{
    public interface IMqttPayload  {
        DateTimeOffset Time { get; }
    }

    public abstract class MqttPayload : IMqttPayload
    {

        [JsonPropertyName("time")]
        public DateTimeOffset Time { get; set;}

    }

    public static class MqttPayloadExtensions
    {
        public static T AsMqttPayload<T>(this MqttApplicationMessage message) where T : IMqttPayload
        {
            T? payload = default(T);
            if (message?.PayloadSegment == null) {
                throw new Exception("Unable to deserialize ApplicationMessage payload");
            } else {
                # pragma warning disable CS8600
                try {
                    payload =  JsonSerializer.Deserialize<T>(message.PayloadSegment, LocalJsonOptions.GetOptions());
                } catch (Exception) {
                    throw new Exception($"Unable to deserialize ApplicationMessage payload: {message.ConvertPayloadToString()}");
                }

                # pragma warning restore CS8600
                if (payload == null) {
                    throw new Exception($"Unable to deserialize ApplicationMessage payload: {message.ConvertPayloadToString()}");
                }
                return payload;
            }
        }

        public static byte[] AsPayload<T>(this T payload) where T : IMqttPayload
        {
            return JsonSerializer.SerializeToUtf8Bytes(payload, LocalJsonOptions.GetOptions());
        }

        public static string ToJsonString<T>(this T payload) where T : IMqttPayload
        {
            return JsonSerializer.Serialize(payload, LocalJsonOptions.GetOptions());
        }

        public static MqttApplicationMessage AsApplicationMessage<T>(
            this T payload, 
            string topic, 
            MqttQualityOfServiceLevel qos = MqttQualityOfServiceLevel.AtLeastOnce) where T : IMqttPayload
        {
            return new MqttApplicationMessageBuilder()
                .WithTopic(topic)
                .WithPayload(payload.AsPayload<T>())
                .WithQualityOfServiceLevel(qos)
                .Build();
        }
    }
    
}

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
        public DateTimeOffset Time { get; } = DateTimeOffset.UtcNow;

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
                    // Use data annotations and customer JsonConverters to Validate the payload 
                    payload =  JsonSerializer.Deserialize<T>(message.PayloadSegment, LocalJsonOptions.DefaultOptions);
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

        // public static byte[] AsPayload<T>(this T payload) where T : IMqttPayload
        // {
        //     return JsonSerializer.SerializeToUtf8Bytes(payload, LocalJsonOptions.GetOptions());
        // }

        public static byte[] AsPayload(this IMqttPayload payload)
        {
            return JsonSerializer.SerializeToUtf8Bytes(payload, payload.GetType(), LocalJsonOptions.DefaultOptions);
        }

        public static string ToJsonString<T>(this T payload) where T : IMqttPayload
        {
            return JsonSerializer.Serialize(payload, LocalJsonOptions.DefaultOptions);
        }

        public static MqttApplicationMessage AsApplicationMessage(
            this IMqttPayload payload, 
            string topic, 
            MqttQualityOfServiceLevel qos = MqttQualityOfServiceLevel.AtLeastOnce)
        {
            return new MqttApplicationMessageBuilder()
                .WithTopic(topic)
                .WithPayload(payload.AsPayload())
                .WithQualityOfServiceLevel(qos)
                .Build();
        }
    }
    
}

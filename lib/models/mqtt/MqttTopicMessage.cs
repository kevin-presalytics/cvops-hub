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

    public interface IMqttDto :  IMqttPayload
    {
        string ResponseTopic { get; set; }
    }

    public abstract class MqttPayload : IMqttPayload
    {

        [JsonPropertyName("time")]
        public DateTimeOffset Time { get; } = DateTimeOffset.UtcNow;
    }
    

    public abstract class MqttDto<T> : MqttPayload where T : class
    {
        [JsonPropertyName("responseTopic")]
        public string ResponseTopic { get; set; } = null!;
        [JsonPropertyName("payload")]
        public T Payload { get; set; } = null!;
    }

    public abstract class MqttDto : MqttDto<object>
    {
    }

    public static class MqttPayloadExtensions
    {
        public static T AsMqttPayload<T>(this MqttApplicationMessage message) where T : IMqttPayload
        {
            T? payload;
            if (message?.PayloadSegment == null) {
                throw new Exception("Unable to deserialize ApplicationMessage payload");
            } else {
                try {
                    // Use data annotations and customer JsonConverters to Validate the payload 
                    payload =  JsonSerializer.Deserialize<T>(message.PayloadSegment, LocalJsonOptions.DefaultOptions);
                    if (payload != null) {                        
                        if (payload is IMqttDto && message.ResponseTopic != null && payload != null) {
                            #pragma warning disable CS8602
                            (payload as IMqttDto).ResponseTopic = message.ResponseTopic;
                            #pragma warning restore CS8602
                        }
                    }
                } catch (Exception) {
                    throw new Exception($"Unable to deserialize ApplicationMessage payload: {message.ConvertPayloadToString()}");
                }
                if (payload == null) {
                    throw new Exception($"Unable to deserialize ApplicationMessage payload: {message.ConvertPayloadToString()}");
                }
                return payload;
            }
        }

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
            if (payload == null) throw new ArgumentNullException(nameof(payload));
            var builder = new MqttApplicationMessageBuilder()
                .WithTopic(topic)
                .WithPayload(payload.AsPayload())
                .WithQualityOfServiceLevel(qos);
            
            if (payload is IMqttDto) {
                #pragma warning disable CS8602
                builder.WithResponseTopic((payload as IMqttDto).ResponseTopic);
                #pragma warning restore CS8602
            }
            return builder.Build();
        }
    }
    
}

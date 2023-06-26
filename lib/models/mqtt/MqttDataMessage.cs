
using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace lib.models.mqtt
{
    public interface IDeviceData {
        Guid DeviceId { get; set; }
    }

    public class  MqttDataMessage<T> : MqttPayload where T : IDeviceData
    {
        
        [JsonPropertyName("type")]
        public MqttDataMessageType DataType { get; set;} = MqttDataMessageType.None;

        [JsonPropertyName("data")]
        public T Data { get; set; } = default!;
    }

    public enum MqttDataMessageType {
        None,
        DeviceRegistered,
        DeviceUnregistered,
        InferenceResult,
    }

    public static class MqttDataMessageTypeExtensions {
        public static MqttDataMessageType GreedyParseMqttDataMessageType (this string value) {
            if (Enum.TryParse<MqttDataMessageType>(value, true, out var result)) {
                return result;
            }  else {
                foreach (MqttDataMessageType enumValue in Enum.GetValues(typeof(MqttDataMessageType))) {
                    if (enumValue.ToString().Equals(value, StringComparison.InvariantCultureIgnoreCase)) {
                        return enumValue;
                    }
                }
            }
            return MqttDataMessageType.None;
        }
    }

    public abstract class  DeviceData : EventArgs, IDeviceData
    {
        [JsonPropertyName("deviceId")]
        public Guid DeviceId { get; set; } = Guid.Empty;
    }

    public class DeviceUnregisteredData : DeviceData {}

    public class DeviceRegisteredData : DeviceData
    {
        [JsonPropertyName("deviceName")]
        public string DeviceName { get; set; } = string.Empty;

        [JsonPropertyName("deviceDescription")]
        public string DeviceDescription { get; set; } = string.Empty;
    }

    public class InferenceResultData : DeviceData
    {
        [JsonPropertyName("inferenceResult")]
        public string InferenceResult { get; set; } = string.Empty;
    }


    public class MqttDataMessageJsonConverter : JsonConverter<MqttDataMessage<IDeviceData>>
    {
        public override bool CanConvert(Type typeToConvert)
        {
            if (!typeToConvert.IsGenericType)
            {
                return false;
            }

            if (typeToConvert.GetGenericTypeDefinition() != typeof(MqttDataMessage<>))
            {
                return false;
            }
            return true;
        }
     
        public override MqttDataMessage<IDeviceData> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            
            var message = new MqttDataMessage<IDeviceData>();
            JsonDocument? dataCache = null;
            while (reader.Read()) {
                if (reader.TokenType == JsonTokenType.EndObject) {
                    break;
                }
                if (reader.TokenType == JsonTokenType.PropertyName) {
                    var propertyName = reader.GetString();
                    reader.Read();
                    switch (propertyName) {
                        case "timestamp":
                            message.Timestamp = DateTimeOffset.FromUnixTimeMilliseconds(reader.GetInt64());
                            break;
                        case "type":
                            string? dataType = reader.GetString();
                            if (dataType == null) {
                                throw new Exception("Unable to deserialize ApplicationMessage payload.  Missing 'type' property");
                            } else {
                                message.DataType = dataType.GreedyParseMqttDataMessageType();
                            }
                            break;
                        case "data":
                            if (message.DataType == MqttDataMessageType.None) {
                                dataCache = JsonDocument.ParseValue(ref reader);
                            } else {
                                switch (message.DataType) {
                                    case MqttDataMessageType.DeviceRegistered:
                                    # pragma warning disable CS8601
                                        message.Data = JsonSerializer.Deserialize<DeviceRegisteredData>(ref reader, options);
                                        break;
                                    case MqttDataMessageType.DeviceUnregistered:
                                        message.Data = JsonSerializer.Deserialize<DeviceUnregisteredData>(ref reader, options);
                                        break;
                                    case MqttDataMessageType.InferenceResult:
                                        message.Data = JsonSerializer.Deserialize<InferenceResultData>(ref reader, options);
                                        break;
                                    # pragma warning restore CS8601
                                    default:
                                        throw new Exception($"Unknown message type: {message.DataType}");
                                }
                                break;
                            }
                            break;
                        default:
                            throw new Exception($"Unknown property: {propertyName}");
                    }
                }
            }
            if (message.Data == null) {
                if (dataCache != null) {
                    switch (message.DataType) {
                        case MqttDataMessageType.DeviceRegistered:
                        #pragma warning disable CS8601
                            message.Data = JsonSerializer.Deserialize<DeviceRegisteredData>(dataCache.RootElement.GetRawText(), options);
                            break;
                        case MqttDataMessageType.DeviceUnregistered:
                            message.Data = JsonSerializer.Deserialize<DeviceUnregisteredData>(dataCache.RootElement.GetRawText(), options);
                            break;
                        case MqttDataMessageType.InferenceResult:
                            message.Data = JsonSerializer.Deserialize<InferenceResultData>(dataCache.RootElement.GetRawText(), options);
                            break;
                        # pragma warning restore CS8601
                        default:
                            throw new Exception($"Unknown message type: {message.DataType}");
                    }
                } else {
                    throw new Exception($"Missing data property");
                }
            }
            return message;

        }

        public override void Write(Utf8JsonWriter writer, MqttDataMessage<IDeviceData> value, JsonSerializerOptions options)
        {
            writer.WriteStartObject();
            writer.WriteString("timestamp", value.Timestamp);
            writer.WriteString("type", value.DataType.ToString());
            writer.WritePropertyName("data");
            JsonSerializer.Serialize(writer, value.Data, options);
            writer.WriteEndObject();
        }
    }
}
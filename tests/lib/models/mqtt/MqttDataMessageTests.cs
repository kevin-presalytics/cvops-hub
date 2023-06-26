using Xunit;
using System;
using lib.extensions;
using lib.models.mqtt;
using System.Text.Json;
using FluentAssertions;

namespace tests.lib.models.mqtt
{
    public class MqttDataMessageTests
    {
        [Theory]
        [InlineData("deviceUnregistered", typeof(DeviceUnregisteredData), MqttDataMessageType.DeviceUnregistered)]
        [InlineData("deviceRegistered", typeof(DeviceRegisteredData), MqttDataMessageType.DeviceRegistered)]
        [InlineData("inferenceResult", typeof(InferenceResultData), MqttDataMessageType.InferenceResult)]
        public void MqttDataMessage_Deserializes_IntoCorrectType(string typeName, Type dataType, MqttDataMessageType messageType)
        {
            Guid deviceId = Guid.NewGuid();
            long timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            string dataString = "{\"deviceId\": \"" + deviceId.ToString() + "\"}";
            string jsonString1 = "{{\"timestamp\": {0}, \"type\": \"{1}\", \"data\": {2} }}";
            string message1 = string.Format(jsonString1, timestamp, typeName, dataString);

            var mqttDataMessage1 = JsonSerializer.Deserialize<MqttDataMessage<IDeviceData>>(message1, LocalJsonOptions.GetOptions());

            //  Ensure json parses correctly independently of order of properties
            string jsonString2 = "{{\"data\": {2}, \"timestamp\": {0}, \"type\": \"{1}\" }}";
            string message2 = string.Format(jsonString2, timestamp, typeName, dataString);
            var mqttDataMessage2 = JsonSerializer.Deserialize<MqttDataMessage<IDeviceData>>(message2, LocalJsonOptions.GetOptions());


            Assert.NotNull(mqttDataMessage1);
            mqttDataMessage1?.Data.Should().BeOfType(dataType);
            mqttDataMessage1?.Data.DeviceId.Should().Be(deviceId);
            mqttDataMessage1?.DataType.Should().Be(messageType);

            Assert.NotNull(mqttDataMessage2);
            mqttDataMessage2?.Data.Should().BeOfType(dataType);
            mqttDataMessage2?.Data.DeviceId.Should().Be(deviceId);
            mqttDataMessage2?.DataType.Should().Be(messageType);

            mqttDataMessage1.Should().BeEquivalentTo(mqttDataMessage2);
        }
    }
}
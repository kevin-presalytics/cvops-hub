using lib.services.mqtt;
using lib.models.db;
using lib.models.dto;

namespace lib.extensions
{
    public static class DeviceExtensions
    {
        public static string GetDataTopic(this Device device) => MqttTopicManager.GetDeviceDataTopic(device.Id);
        public static string GetCommandTopic(this Device device) => MqttTopicManager.GetDeviceCommandTopic(device.Id);
        public static string GetStatusTopic(this Device device) => MqttTopicManager.GetDeviceStatusTopic(device.Id);
    }
}
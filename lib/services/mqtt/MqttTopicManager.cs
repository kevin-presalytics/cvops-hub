using System;

namespace lib.services.mqtt
{
    public static class MqttTopicManager
    {
        public static string GetDeviceDataTopic(Guid deviceId) => $"device/{deviceId}/data";
        public static string GetDeviceCommandTopic(Guid deviceId) => $"device/{deviceId}/command";
        public static string GetDeviceStatusTopic(Guid deviceId) => $"device/{deviceId}/status";
        public static string GetHubApiTopic() => $"hub/api";
        public static string GetHubControllerTopic() => $"hub/controller";
        public static string GetHubWorkerTopic(string channelName) => $"hub/worker/{channelName}";
    }
}
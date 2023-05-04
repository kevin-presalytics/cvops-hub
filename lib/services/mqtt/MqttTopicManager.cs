using System;
using MQTTnet;
using lib.models.mqtt;

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
        public static string GetUserLoginTopic(Guid userId) => $"user/{userId}/login";
        public static string GetUserNotificationTopic(Guid userId) => $"user/{userId}/notification";

        public static MqttTopicType GetTopicType(this MqttApplicationMessage message) => MqttTopicExtensions.GetTopicType(message.Topic);

    }

    public static class MqttTopicExtensions
    {
        public static MqttTopicType GetTopicType(string topic)
        {
            string[] topicParts = topic.Split('/');
            if (topicParts[0] == "hub") {
                if (topicParts[1] == "api") return MqttTopicType.HubApi;
                else if (topicParts[1] == "controller") return MqttTopicType.HubController;
                else if (topicParts[1] == "worker") return MqttTopicType.HubWorker;
                else throw new Exception("Invalid topic type");
            } else if (topicParts[0] == "device") {
                if (topicParts[2] == "data") return MqttTopicType.DeviceData;
                else if (topicParts[2] == "command") return MqttTopicType.DeviceCommand;
                else if (topicParts[2] == "status") return MqttTopicType.DeviceStatus;
                else throw new Exception("Invalid topic type");
            } else if (topicParts[0] == "user") {
                if (topicParts[2] == "login") return MqttTopicType.UserLogin;
                else if (topicParts[2] == "notification") return MqttTopicType.UserNotification;
                else throw new Exception("Invalid topic type");
            } else {
                throw new Exception("Invalid topic type");
            }
        }
    }
}
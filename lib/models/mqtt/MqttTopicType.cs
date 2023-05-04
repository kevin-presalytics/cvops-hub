namespace lib.models.mqtt
{
    public enum MqttTopicType
    {
        DeviceData,
        DeviceCommand,
        DeviceStatus,
        HubApi,
        HubController,
        HubWorker,
        UserLogin,
        UserNotification,
    }
}
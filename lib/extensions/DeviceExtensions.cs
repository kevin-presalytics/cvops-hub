using lib.services.mqtt;
using lib.models.db;
using dto = lib.models.dto;

namespace lib.extensions
{
    public static class DeviceExtensions
    {
        public static string GetDataTopic(this Device device) => MqttTopicManager.GetDeviceDataTopic(device.Id);
        public static string GetCommandTopic(this Device device) => MqttTopicManager.GetDeviceCommandTopic(device.Id);
        public static string GetStatusTopic(this Device device) => MqttTopicManager.GetDeviceStatusTopic(device.Id);
        public static dto.Device ToDto(this Device device) => new dto.Device() {
            Id = device.Id,
            Name = device.Name,
            DeviceInfo = device.DeviceInfo,
            WorkspaceId = device.WorkspaceId,
            Description = device.Description,
            CreatedBy = device.CreatedBy,
            DateCreated = device.DateCreated,
            ModifiedBy = device.ModifiedBy,
            DateModified = device.DateModified,
            UserCreated = device.UserCreated,
            UserModified = device.UserModified,
            ActivationStatus = device.ActivationStatus,
        };
    }
}
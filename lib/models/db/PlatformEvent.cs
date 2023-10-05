using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;
using System;
using System.Text.Json;
using lib.models.mqtt;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace lib.models.db
{
    [Keyless]
    [Table("platform_events")]
    public class PlatformEvent : TimeSeriesEntity, IMqttPayload
    {
        [JsonConverter(typeof(PlatformEventTypeJsonConverter))]
        public PlatformEventTypes EventType {get; set;}
        public Guid WorkspaceId {get; set;} 
        public Guid? UserId {get; set;}
        public Guid? DeviceId {get; set;}
        
        [Column(TypeName = "jsonb")]
        public JsonDocument EventData {get; set;} =  JsonDocument.Parse("{}");
        public string? ResponseTopic { get; set; } = null;
    }

    public enum PlatformEventTypes {
        None,
        UserLogin,
        UserLogout,
        UserNotification,
        DeviceRegistered,
        DeviceUnregistered,
        DeviceUpdated,
        DeviceDetailsRequest,
        DeviceDetailsResponse,
        WorkspaceDetailsRequest,
        WorkspaceDetailsResponse,
        WorkspaceCreated,
        WorkspaceDeleted,
        WorkspaceUpdated,
        WorkspaceUserAdded,
        WorkspaceUserRemoved,
        DeviceAdded,
        DeviceRemoved,
        DeploymentCreated,
        DeploymentUpdated,
        DeploymentDeleted,
    }

    public class PlatformEventTypeJsonConverter : JsonConverter<PlatformEventTypes>
    {
        public static Dictionary<PlatformEventTypes, string> SerializedEventTypeMap = new Dictionary<PlatformEventTypes, string>() {
            {PlatformEventTypes.UserLogin, "user.login"},
            {PlatformEventTypes.UserLogout, "user.logout"},
            {PlatformEventTypes.UserNotification, "user.notification"},
            {PlatformEventTypes.DeviceRegistered, "device.registered"},
            {PlatformEventTypes.DeviceUnregistered, "device.unregistered"},
            {PlatformEventTypes.DeviceUpdated, "device.details_update"},
            {PlatformEventTypes.DeviceDetailsRequest, "device.details_request"},
            {PlatformEventTypes.DeviceDetailsResponse, "device.details_response"},
            {PlatformEventTypes.WorkspaceCreated, "workspace.created"},
            {PlatformEventTypes.WorkspaceDeleted, "workspace.deleted"},
            {PlatformEventTypes.WorkspaceUpdated, "workspace.updated"},
            {PlatformEventTypes.WorkspaceUserAdded, "workspace.user_added"},
            {PlatformEventTypes.WorkspaceUserRemoved, "workspace.user_removed"},
            {PlatformEventTypes.WorkspaceDetailsRequest, "workspace.details_request"},
            {PlatformEventTypes.WorkspaceDetailsResponse, "workspace.details_response"},
            {PlatformEventTypes.DeviceAdded, "device.added"},
            {PlatformEventTypes.DeviceRemoved, "device.removed"},
            {PlatformEventTypes.DeploymentCreated, "deployment.created"},
            {PlatformEventTypes.DeploymentUpdated, "deployment.updated"},
            {PlatformEventTypes.DeploymentDeleted, "deployment.deleted"},
        };

        public override PlatformEventTypes Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType != JsonTokenType.String) throw new JsonException();

            var eventTypeString = reader.GetString();
            foreach (var kvp in SerializedEventTypeMap) {
                if (kvp.Value == eventTypeString) {
                    return kvp.Key;
                }
            }
            return PlatformEventTypes.None;
        }

        public override void Write(Utf8JsonWriter writer, PlatformEventTypes value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(SerializedEventTypeMap[value]);
        }
    }
}


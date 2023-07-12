using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;
using System;
using System.Text.Json;
using lib.models.mqtt;

namespace lib.models.db
{
    [Keyless]
    [Table("platform_events")]
    public class PlatformEvent : TimeSeriesEntity, IMqttPayload
    {
        public PlatformEventTypes EventType {get; set;}
        public Guid WorkspaceId {get; set;} 
        public Guid? UserId {get; set;}
        public Guid? DeviceId {get; set;}
        
        [Column(TypeName = "jsonb")]
        public JsonDocument EventData {get; set;} =  JsonDocument.Parse("{}");
    }

    public enum PlatformEventTypes {
        None,
        UserLogin,
        UserLogout,
        UserNotification,
        DeviceRegistered,
        DeviceUnregistered,
    }
}


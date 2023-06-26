using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;
using System;
using System.Text.Json;

namespace lib.models.db
{
    [Keyless]
    public class PlatformEvent
    {
        public DateTimeOffset Time {get; set;} = DateTimeOffset.UtcNow;
        public PlatformEventTypes EventType {get; set;}
        public Guid WorkspaceId {get; set;} 
        public Guid? UserId {get; set;}
        public Guid? DeviceId {get; set;}

        [Column(TypeName = "jsonb")]
        public JsonDocument EventData {get; set;} =  JsonDocument.Parse("{}");
    }

    public enum PlatformEventTypes {
        UserLogin,
        UserLogout,
        DeviceREgistered,
        DeviceUnregistered,
    }
}
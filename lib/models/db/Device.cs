// EntityFramework POCO class for table 'device' in database 'db'
// Contains fields for id, name, description, and device_type_id, salt, and hash
using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.ComponentModel.DataAnnotations.Schema;

namespace lib.models.db
{
    public class Device : BaseEntity
    {
        public string Name { get; set; } = default!;
        public string Description { get; set; } =  default!;
        [Column(TypeName = "jsonb")]
        public JsonDocument DeviceInfo { get; set; } = JsonDocument.Parse("{}");
        public byte[] Salt { get; set; } = default!;
        public string Hash { get; set; } = default!;
        public Guid WorkspaceId { get; set; } = default!;
        [JsonIgnore]
        public virtual Workspace Workspace { get; set; } = default!;
        public DeviceActivationStatus ActivationStatus { get; set; } = DeviceActivationStatus.Inactive;
    }

    public enum DeviceActivationStatus
    {
        Inactive,
        Active,
        Deleted
    }
}
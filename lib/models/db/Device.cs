// EntityFramework POCO class for table 'device' in database 'db'
// Contains fields for id, name, description, and device_type_id, salt, and hash
using System;
using System.Text.Json;
using System.ComponentModel.DataAnnotations.Schema;

namespace lib.models.db
{
    public class Device : BaseEntity
    {
        public string? Name { get; set; } = default!;
        public string? Description { get; set; } =  default!;
        [Column(TypeName = "jsonb")]
        public JsonDocument DeviceInfo { get; set; } = default!;
        public byte[] Salt { get; set; } = default!;
        public string Hash { get; set; } = default!;
        public Guid TeamId { get; set; } = default!;
        public virtual Team Team { get; set; } = default!;


    }
}
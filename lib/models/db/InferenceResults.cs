using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;
using System;
using System.Text.Json;

namespace lib.models.db
{
    [Keyless]
    public class InferenceResult
    {
        public DateTimeOffset Time {get; set;} = DateTimeOffset.UtcNow;
        [ForeignKey("Workspace")]
        public Guid WorkspaceId {get; set;}
        [ForeignKey("Device")]
        public Guid DeviceId {get; set;}
        [Column(TypeName = "jsonb")]

        public InferenceResultTypes ResultType {get; set;}
        public JsonDocument? Boxes {get; set;}
        [Column(TypeName = "jsonb")]
        public JsonDocument? Meshes {get; set;}
        [Column(TypeName = "jsonb")]
        public JsonDocument Labels {get; set;} = JsonDocument.Parse("{}");

    }

    public enum InferenceResultTypes {
        Boxes,
        Meshes,
        Labels
    }
}
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;
using System;
using System.Text.Json;
using lib.models.mqtt;

namespace lib.models.db
{
    [Keyless]
    [Table("inference_results")]
    public class InferenceResult : TimeSeriesEntity, IMqttPayload
    {
        public Guid WorkspaceId {get; set;}
        public Guid DeviceId {get; set;}
        public InferenceResultTypes ResultType {get; set;}
        
        [Column(TypeName = "jsonb")]
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
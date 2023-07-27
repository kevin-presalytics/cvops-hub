using System;
using System.Collections.Generic;
using lib.models.db;
using lib.models.mqtt;
using System.Text.Json.Serialization;

namespace lib.models.dto
{

    public class DeploymentMessage : IMqttPayload
    {
        public DateTimeOffset Time { get; set;} = DateTimeOffset.UtcNow;
        
        [JsonConverter(typeof(JsonStringEnumConverter))]
        [JsonPropertyName("type")]
        public DeploymentMessageTypes MessageType { get; set;} = DeploymentMessageTypes.None;
        public object Payload { get; set;} = default!;
    }

    public enum DeploymentMessageTypes
    {
        None,
        Created,
        Updated,
        Deleted
    }


    public class DeploymentCreatedPayload
    {
        public Guid WorkspaceId { get; set;} = default!;
        public List<Guid> DeviceIds { get; set;} = new List<Guid>();

    }

    public class DeploymentUpdatedPayload
    {
        public Deployment Deployment { get; set;} = default!;
    }

    public class DeploymentDeletedPayload
    {
        public Guid DeploymentId { get; set;} = default!;
    }
}
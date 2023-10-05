using System;
using System.Collections.Generic;
using db = lib.models.db;
using lib.models;
using lib.models.mqtt;
using System.Text.Json.Serialization;
using System.Text.Json;

namespace lib.models.dto
{

    public class DeploymentMessage : MqttDto
    {
        
        [JsonConverter(typeof(JsonStringEnumConverter))]
        [JsonPropertyName("type")]
        public DeploymentMessageTypes MessageType { get; set;} = DeploymentMessageTypes.None;
    }

    public enum DeploymentMessageTypes
    {
        None,
        Created,
        Updated,
        Deleted,
        DeviceStatus,
    }


    public class DeploymentCreatedPayload
    {
        public Guid WorkspaceId { get; set;} = default!;
        public List<Guid> DeviceIds { get; set;} = new List<Guid>();
        public db.DeploymentSources ModelSource {get; set;} = db.DeploymentSources.LocalFile;
        public db.ModelTypes ModelType { get;set; } = db.ModelTypes.ImageSegmentation;
        public Guid DeploymentInitiatorId { get; set;} = default!;
        public EditorTypes DeploymentInitiatorType { get; set;} = EditorTypes.User;
        public string? BucketName {get; set;} = default!;
        public string? ObjectName {get; set;} = default!;
        public JsonDocument? ModelMetadata {get; set;} = JsonDocument.Parse("{}");
    }

    public class DeploymentDeletedPayload
    {
        public Guid DeploymentId { get; set;} = default!;
    }

    public class Deployment : BaseEntity
    {
        public db.DeploymentSources Source {get; set;} = db.DeploymentSources.LocalFile;
        public Guid WorkspaceId {get; set;} = default!;
        public string BucketName {get; set;} = default!;
        public string ObjectName {get; set;} = default!;
        public db.ModelTypes ModelType { get;set; } = db.ModelTypes.ImageSegmentation;
        public db.DeploymentStatus Status {get; set;} = db.DeploymentStatus.None;
        public JsonDocument DevicesStatus {get; set;} = JsonDocument.Parse("{}");
        public JsonDocument ModelMetadata {get; set;} = JsonDocument.Parse("{}");
    }

    public class DeviceDeploymentStatus
    {
        public Guid DeploymentId { get; set;} = default!;
        public Guid DeviceId { get; set;} = default!;
        public DeviceDeploymentStatusTypes Status { get; set;} = DeviceDeploymentStatusTypes.None;
        public string? Message { get; set;} = default!;
    }

    public enum DeviceDeploymentStatusTypes
    {
        None,
        WaitingForModel,
        Downloading,
        Downloaded,
        Active,
        Obsolete,
        Failed
    }

    public class DevicesStatus
    {
        public List<DeviceDeploymentStatus> Devices { get; set;} = new List<DeviceDeploymentStatus>();
    }

    
}
using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace lib.models.db
{
    public class Deployment : BaseEntity
    {
        public Guid DeploymentInitiatorId { get; set;} = default!;
        public EditorTypes DeploymentInitiatorType { get; set;} = EditorTypes.User;
        public DeploymentSources ModelSource {get; set;} = DeploymentSources.LocalFile;
        public Guid WorkspaceId {get; set;} = default!;
        [JsonIgnore]
        public virtual Workspace Workspace {get; set;} = default!;
        public string BucketName {get; set;} = default!;
        public string ObjectName {get; set;} = default!;
        public ModelTypes ModelType { get;set; } = ModelTypes.ImageSegmentation;
        public DeploymentStatus Status {get; set;} = DeploymentStatus.None;
        public JsonDocument DevicesStatus {get; set;} = JsonDocument.Parse("{}");
        public JsonDocument ModelMetadata {get; set;} = JsonDocument.Parse("{}");
    }

    public enum DeploymentSources
    {
        LocalFile,
        RemoteS3Bucket, // Not Implemented
        WeightsAndBiases, // Not Implemented
        MlFlow, // Not Implemented

    }

    public enum ModelTypes
    {
        ImageSegmentation,
        ImageClassification, // Not Implemented
        ObjectDetection, // Not Implemented
        ChatBot,  // Not Implemented
    }

    public enum DeploymentStatus
    {
        None,
        Created,
        ModelUploading,
        ModelUploaded,
        ModelValidated,
        ModelDeploying,
        ModelDeployed,
        Active,
        Failed,
        Deleted
    }
}
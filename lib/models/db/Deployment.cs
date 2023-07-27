using System;
using System.Text.Json;

namespace lib.models.db
{
    public class Deployment : BaseEntity
    {
        public DeploymentSources Source {get; set;} = DeploymentSources.LocalS3Bucket;
        public Guid WorkspaceId {get; set;} = default!;
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
        LocalS3Bucket,
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
        ModelUploaded,
        ModelValidated,
        ModelDeployed,
        Active,
        Failed,
        Deleted
    }
}
using lib.models.db;
using dto = lib.models.dto;
using System;

namespace lib.extensions
{
public static class DeploymentExtensions
    {
        public static dto.Deployment ToDto(this Deployment deployment)
        {
            return new dto.Deployment
            {
                Id = deployment.Id,
                ModelSource = deployment.ModelSource,
                WorkspaceId = deployment.WorkspaceId,
                BucketName = deployment.BucketName,
                ObjectName = deployment.ObjectName,
                ModelType = deployment.ModelType,
                Status = deployment.Status,
                DevicesStatus = deployment.DevicesStatus,
                ModelMetadata = deployment.ModelMetadata
            };
        }

        public static Guid GetDeploymentId<T>(this dto.DeploymentMessage<T> message) where T : class
        {
            T payload = message.Payload;
            Guid id;
            if (payload is dto.Deployment) {
                #pragma warning disable CS8602, CS8600 // Possible null reference argument.
                dto.Deployment deployment = payload as dto.Deployment;
                id = deployment.Id;
                #pragma warning restore CS8602, CS8600 // Possible null reference argument.
            } else if (payload is dto.DeploymentDeletedPayload) {
                #pragma warning disable CS8602, CS8600 // Possible null reference argument.
                dto.DeploymentDeletedPayload deleted = payload as dto.DeploymentDeletedPayload;
                id = deleted.DeploymentId;
                #pragma warning restore CS8602, CS8600 // Possible null reference argument.
            } else if (payload is dto.DeviceDeploymentStatus) {
                #pragma warning disable CS8602, CS8600 // Possible null reference argument.
                dto.DeviceDeploymentStatus deviceStatus = payload as dto.DeviceDeploymentStatus;
                id = deviceStatus.DeploymentId;
                #pragma warning restore CS8602, CS8600 // Possible null reference argument.
            } else {
                throw new Exception("Deployment message type has not Deployment Id.");
            }
            if (id == Guid.Empty) {
                throw new Exception("Deployment Id is empty.");
            }
            return id;
        }
    }
}
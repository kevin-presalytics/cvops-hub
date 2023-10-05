using lib.models.db;
using dto = lib.models.dto;

namespace lib.extensions
{
public static class DeploymentExtensions
    {
        public static dto.Deployment ToDto(this Deployment deployment)
        {
            return new dto.Deployment
            {
                Id = deployment.Id,
                Source = deployment.ModelSource,
                WorkspaceId = deployment.WorkspaceId,
                BucketName = deployment.BucketName,
                ObjectName = deployment.ObjectName,
                ModelType = deployment.ModelType,
                Status = deployment.Status,
                DevicesStatus = deployment.DevicesStatus,
                ModelMetadata = deployment.ModelMetadata
            };
        }
    }
}
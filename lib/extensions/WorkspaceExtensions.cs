using lib.services.mqtt;
using lib.models.db;
using dto = lib.models.dto;
using System.Linq;

namespace lib.extensions
{
    public static class WorkspaceExtensions
    {
        public static dto.Workspace ToDto(this Workspace workspace) => new dto.Workspace() {
            Id = workspace.Id,
            Name = workspace.Name,
            CreatedBy = workspace.CreatedBy,
            DateCreated = workspace.DateCreated,
            ModifiedBy = workspace.ModifiedBy,
            DateModified = workspace.DateModified,
            UserCreated = workspace.UserCreated,
            UserModified = workspace.UserModified,
            Description = workspace.Description,
            Users = workspace.GetUserDTOs(),
            Deployments = workspace.Deployments.ToList(),
            Devices = workspace.Devices.Select(d => d.ToDto()).ToList()
        };

        public static dto.UserWorkspace ToUserWorkspace(this Workspace workspace, User currentUser) {
            return new dto.UserWorkspace() {
                Id = workspace.Id,
                Name = workspace.Name,
                CreatedBy = workspace.CreatedBy,
                DateCreated = workspace.DateCreated,
                ModifiedBy = workspace.ModifiedBy,
                DateModified = workspace.DateModified,
                UserCreated = workspace.UserCreated,
                UserModified = workspace.UserModified,
                Description = workspace.Description,
                IsDefault = currentUser.DefaultWorkspaceId == workspace.Id,
            };
        }
    }
}
using lib.services.mqtt;
using lib.models.db;
using dto = lib.models.dto;
using System.Collections.Generic;
using System.Linq;

namespace lib.extensions
{
    public static class UserExtensions
    {
        public static List<dto.User> GetUserDTOs(this Workspace workspace) {
            return workspace.WorkspaceUsers.Select(u => {
                return new dto.User() {
                    Id = u.UserId,
                    Email = u.User.Email,
                    Role = u.WorkspaceUserRole
                };
            }).ToList();
        }

        public static dto.User ToDto(this User user, WorkspaceUserRole? role = null)
        {
            return new dto.User
            {
                Id = user.Id,
                Email = user.Email,
                Role = role,
            };
    
        }
    }
}
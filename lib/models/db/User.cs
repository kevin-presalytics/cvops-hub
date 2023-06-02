using System;
using System.ComponentModel.DataAnnotations;

namespace lib.models.db
{
    public class User : BaseEntity
    {
        [EmailAddress]
        public string Email { get; set; } = default!;
        public string JwtSubject { get; set; } = default!;
        public Guid DefaultWorkspaceId { get; set; } = default!;
        public virtual Workspace DefaultWorkspace { get; set; } = default!;
        public bool IsEmailVerified { get; set; } = default!;
        public UserStatus Status { get; set; } = default!;
    }

    public enum UserStatus {
        Active,
        Pending,
        Inactive,
        Deleted
    }
}
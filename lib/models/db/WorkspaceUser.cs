using System;

namespace lib.models.db
{
    public class WorkspaceUser : BaseEntity
    {
        public Guid WorkspaceId { get; set; } = default!;
        public virtual Workspace Workspace { get; set; } = default!;
        public Guid UserId { get; set; } = default!;
        public virtual User User { get; set;} = default!;
        public WorkspaceUserRole WorkspaceUserRole { get; set; } = default!;
        

    }

    public enum WorkspaceUserRole
    {
        Owner,
        Editor,
        Viewer,
    }
}
using System;
using System.ComponentModel.DataAnnotations;
using lib.models.db;

namespace lib.models.dto
{
    public class Workspace : BaseEntity
    {
        public string Name { get; set; } = default!;
        public string Description { get; set; } = default!;
    }

    public class UserWorkspace : Workspace
    {
        public bool IsDefault { get; set; } = default!;
    }
    
    public class NewWorkspaceRequestBody
    {
        public string Name { get; set; } = default!;
        public string Description { get; set; } = default!;
        public bool IsDefault { get; set; } = default!;
    }

    public class UpdateWorkspaceRequestBody : NewWorkspaceRequestBody
    {
        public Guid Id { get; set; } = default!;
    }

    public class NewUserRequestBody
    {
        [EmailAddress]
        public string Email { get; set; } = default!;
        public WorkspaceUserRole Role { get; set; } = WorkspaceUserRole.Viewer;
    }

    public class User : NewUserRequestBody
    {
        public Guid Id { get; set;} = default!;
    }
}
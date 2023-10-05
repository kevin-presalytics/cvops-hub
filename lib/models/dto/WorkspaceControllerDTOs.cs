using System;
using System.ComponentModel.DataAnnotations;
using db = lib.models.db;
using System.Collections.Generic;
using lib.models.mqtt;

namespace lib.models.dto
{
    public class Workspace : BaseEntity
    {
        public string Name { get; set; } = default!;
        public string Description { get; set; } = default!;
        public List<User> Users { get; set;} = default!;
        public List<db.Deployment> Deployments { get; set;} = default!;
        public List<Device> Devices { get; set;} = default!;
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
        public db.WorkspaceUserRole Role { get; set; } = db.WorkspaceUserRole.Viewer;
    }

    public class User
    {
        public Guid Id { get; set;} = default!;
        [EmailAddress]
        public string Email { get; set; } = default!;
        public db.WorkspaceUserRole? Role { get; set; } = null;
    }

    public class WorkspaceDetails : IMqttPayload
    {
        public DateTimeOffset Time => DateTimeOffset.UtcNow;
        public Guid Id { get; set;} = default!;
        public string Name { get; set; } = default!;
        public string Description { get; set; } = default!;
        public List<User> Users { get; set;} = default!;
        public List<Deployment> Deployments { get; set;} = default!;
        public List<Device> Devices { get; set;} = default!;
    }
}
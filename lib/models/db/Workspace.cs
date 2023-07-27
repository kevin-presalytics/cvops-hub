using System.Collections.Generic;

namespace lib.models.db
{
    public class Workspace : BaseEntity
    {
        public string Name { get; set; } = default!;
        public string Description { get; set; } = default!;
        public virtual List<WorkspaceUser> WorkspaceUsers { get; set;} = default!;
        public virtual List<Device> Devices { get; set;} = default!;
        public virtual List<Deployment> Deployments { get; set;} = default!;

    }
}
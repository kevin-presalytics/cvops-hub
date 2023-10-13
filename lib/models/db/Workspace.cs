using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace lib.models.db
{
    public class Workspace : BaseEntity
    {
        public string Name { get; set; } = default!;
        public string Description { get; set; } = default!;
        [JsonIgnore]
        public virtual List<WorkspaceUser> WorkspaceUsers { get; set;} = default!;
        [JsonIgnore]
        public virtual List<Device> Devices { get; set;} = default!;
        [JsonIgnore]
        public virtual List<Deployment> Deployments { get; set;} = default!;

    }
}
using System;
using System.Collections.Generic;

namespace lib.models.db
{
    public class Team : BaseEntity
    {
        public string Name { get; set; } = default!;
        public List<User> Users { get; set;} = default!;
        public List<Device> Devices { get; set;} = default!;
        public bool isDefaultTeam { get; set;} = default!;

    }
}
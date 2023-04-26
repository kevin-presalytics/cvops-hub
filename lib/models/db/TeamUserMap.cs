using System;

namespace lib.models.db
{
    public class TeamUserMap : BaseEntity
    {
        public Guid TeamId { get; set; } = default!;
        public Guid UserId { get; set; } = default!;
    }
}
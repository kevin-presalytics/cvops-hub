using System.Collections.Generic;

namespace lib.models.db
{
    public class User : BaseEntity
    {
        public string Email { get; set; } = default!;
        public string JwtSubject { get; set; } = default!;
        public List<Team> Teams { get; set; } = default!;
    }
}
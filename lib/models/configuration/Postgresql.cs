// postgresql configuration

namespace lib.models.configuration
{
    public class Postgresql
    {
        public string database { get; set; } = default!;
        public string host { get; set; } = default!;
        public string password { get; set; } = default!;
        public string username { get; set; } = default!;
        public int port { get; set; } = default!;
    }
}
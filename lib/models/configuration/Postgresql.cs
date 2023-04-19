// postgresql configuration

namespace lib.models.configuration
{
    public class Postgresql
    {
        public string Database { get; set; } = default!;
        public string Host { get; set; } = default!;
        public string Password { get; set; } = default!;
        public string Username { get; set; } = default!;
        public int Port { get; set; } = default!;
        public string SslMode { get; set; } = default!;
    }
}
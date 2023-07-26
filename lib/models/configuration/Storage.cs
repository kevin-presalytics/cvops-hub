namespace lib.models.configuration
{
    public class Storage
    {
        public bool UseTls { get; set;} = false;
        public string Host { get; set;} = default!;
        public int? Port { get; set; } = null;
        public string AccessKey { get; set;} = default!;
        public string Secret { get; set;} = default!;
        public string PublicHost { get; set; } = default!;
        public int PresignedExpirySeconds { get; set; } = 3600;
    }
}
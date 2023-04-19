namespace lib.models.dto
{
    // DTO for SecureDeviceCredentials containing a key, salt and hash
    public class SecureDeviceCredentials
    {
        public string Key { get; set; } = default!;
        public byte[] Salt { get; set; } = default!;
        public string Hash { get; set; } = default!;
    }
}
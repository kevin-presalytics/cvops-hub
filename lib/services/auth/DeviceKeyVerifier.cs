namespace lib.services.auth
{
    public interface IDeviceKeyVerifier
    {
        bool Verify(string key, string hash, byte[] salt);
    }

    public class DeviceKeyVerifier : IDeviceKeyVerifier
    {
        IDeviceKeyGenerator _generator;
        public DeviceKeyVerifier(IDeviceKeyGenerator generator) 
        {
            this._generator = generator;
        }

        public bool Verify(string key, string hash, byte[] salt)
        {
            var credentials = this._generator.HashPassword(key, salt);
            return credentials.Hash == hash;
        }
    }
}
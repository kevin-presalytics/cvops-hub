using System;
using System.Security.Cryptography;
using lib.models.dto;
using System.Text;
using System.Text.Encodings;

namespace lib.services.auth
{
    // DeviceKeyGenerator Interface with a GenerateKey method
    public interface IDeviceKeyGenerator
    {
        // GenerateKey method
        SecureDeviceCredentials GenerateKey();
        SecureDeviceCredentials HashPassword(string password, byte[] salt);
    }

    // DeviceKeyGenerator class with a GenerateKey method implementing IDeviceKeyGenerator
    public class DeviceKeyGenerator : IDeviceKeyGenerator
    {
        public DeviceKeyGenerator() { }
        // GenerateKey returns a string and has two arg
        public SecureDeviceCredentials GenerateKey()
        {
            // Generate a random key
            const int keySize = 64;
            var key = Guid.NewGuid().ToString();
            var salt = RandomNumberGenerator.GetBytes(keySize);
            return this.HashPassword(key, salt);
        }

        public SecureDeviceCredentials HashPassword(string password, byte[] salt)
            {
            const int iterations = 350000;
            HashAlgorithmName hashAlgorithm = HashAlgorithmName.SHA512;

            var hash = Rfc2898DeriveBytes.Pbkdf2(
                Encoding.UTF8.GetBytes(password),
                salt,
                iterations,
                hashAlgorithm,
                salt.Length);
            return new SecureDeviceCredentials() {
                Key = password,
                Salt = salt,
                Hash = Convert.ToBase64String(hash)
            };
        }
    }
}
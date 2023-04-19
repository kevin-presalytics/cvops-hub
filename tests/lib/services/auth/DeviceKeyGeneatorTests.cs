
using Xunit;
using FluentAssertions;
using lib.services.auth;
using lib.models.dto;

namespace tests.lib.services.auth
{
    public class DeviceKeyGeneratorTests
    {
        [Fact]
        public void TestGenerate()
        {
            // Arrange
            DeviceKeyGenerator generator = new DeviceKeyGenerator();
            DeviceKeyVerifier verifier = new DeviceKeyVerifier(generator);
            // Act
            SecureDeviceCredentials creds = generator.GenerateKey();
            bool result = verifier.Verify(creds.Key, creds.Hash, creds.Salt);

            // Assert
            result.Should().BeTrue();
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Serilog;
using lib.services.auth;
using lib.models;
using lib.models.db;
using lib.models.mqtt;
using lib.models.configuration;


namespace lib.services.mqtt
{
    public interface IMqttHttpAuthenticator
    {
        Task<MqttAuthResponse> Authenticate(string username, string password);
    }

    public class MqttHttpAuthenticator : IMqttHttpAuthenticator
    {
        private readonly CvopsDbContext _dbContext;
        private readonly IDeviceKeyVerifier _deviceKeyVerifier;
        private readonly ILogger _logger;
        private readonly string _hubUsername;
        private readonly string _hubPassword;


        public MqttHttpAuthenticator(
            CvopsDbContext dbContext, 
            IDeviceKeyVerifier deviceKeyVerifier, 
            ILogger logger, 
            AppConfiguration appConfig)
        {
            _dbContext = dbContext;
            _deviceKeyVerifier = deviceKeyVerifier;
            _logger = logger;
            _hubUsername = appConfig.MQTT.AdminUsername;
            _hubPassword = appConfig.MQTT.AdminPassword;
        }
    
        public async Task<MqttAuthResponse> Authenticate(string username, string password)
        {
            try {
                if (username == _hubUsername && password == _hubPassword)
                {
                    return new MqttAuthResponse() {
                        Result = AuthResultOptions.Allow,
                        IsSuperuser = true,
                    };
                }
                Guid userId = Guid.Parse(username);
                if (userId == Guid.Empty)
                {
                    return new MqttAuthResponse() {
                        Result = AuthResultOptions.Deny,
                        IsSuperuser = false,
                    };
                } else {
                    #pragma warning disable CS8600
                    Device device = await _dbContext.Devices.FindAsync(userId);
                    #pragma warning restore CS8600
                    if (device == null)
                        return new MqttAuthResponse() {
                            Result = AuthResultOptions.Deny,
                            IsSuperuser = false,
                        };
                    if (_deviceKeyVerifier.Verify(password, device.Hash, device.Salt))
                    {
                        MqttAuthResponse response = new MqttAuthResponse() {
                            Result = AuthResultOptions.Allow,
                            IsSuperuser = false,

                        };
                        return response;
                    }
                }
                return new MqttAuthResponse() {
                    Result = AuthResultOptions.Deny,
                    IsSuperuser = false,
                };
            } catch (Exception e) {
                _logger.Error(e, "Error authenticating MQTT user");
                return new MqttAuthResponse() {
                    Result = AuthResultOptions.Deny,
                    IsSuperuser = false,
                };
            }
        }
    }
}
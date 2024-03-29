using System;
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
        private readonly CvopsDbContext _context;
        private readonly IDeviceKeyVerifier _deviceKeyVerifier;
        private readonly ILogger _logger;
        private readonly string _hubUsername;
        private readonly string _hubPassword;
        private readonly IUserService _userService;


        public MqttHttpAuthenticator(
            CvopsDbContext dbContext, 
            IDeviceKeyVerifier deviceKeyVerifier, 
            ILogger logger, 
            AppConfiguration appConfig,
            IUserService userService
        )
        {
            _context = dbContext;
            _deviceKeyVerifier = deviceKeyVerifier;
            _logger = logger;
            _hubUsername = appConfig.MQTT.AdminUsername;
            _hubPassword = appConfig.MQTT.AdminPassword;
            _userService = userService;
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
                if (await IsValidDevice(username, password)) {
                    return new MqttAuthResponse() {
                        Result = AuthResultOptions.Allow,
                        IsSuperuser = false,
                    };
                } else if (await IsValidUser(username, password)) {
                    return new MqttAuthResponse() {
                        Result = AuthResultOptions.Allow,
                        IsSuperuser = false,
                    };
                } else {
                    return new MqttAuthResponse() {
                        Result = AuthResultOptions.Deny,
                        IsSuperuser = false,
                    };
                }
            } catch (Exception e) {
                _logger.Error(e, "Error authenticating MQTT user");
                return new MqttAuthResponse() {
                    Result = AuthResultOptions.Deny,
                    IsSuperuser = false,
                };
            }
        }

        private async Task<bool> IsValidDevice(string username, string password)
        {
            Guid userId;
            bool usernameIsGuid = Guid.TryParse(username, out userId);
            if (userId == Guid.Empty || !usernameIsGuid)
            {
                return false;
            }
            # pragma warning disable CS8600
            Device device = await _context.Devices.FindAsync(userId);
            # pragma warning restore CS8600
            if (device == null)
            {
                return false;
            }
            if (_deviceKeyVerifier.Verify(password, device.Hash, device.Salt))
            {
                return true;
            }
            return false;
        }

        private async Task<bool> IsValidUser(string username, string jwtToken)
        {
            try {
                // Assumes users attaching directly hub are passing a jwt as their password and
                // the jwt's subject as the username
                User user = await _userService.GetOrCreateUser(jwtToken);
                if (user.JwtSubject == username) {
                    return true;
                }
                return false;
            } catch (Exception ex) {
                _logger.Error(ex, "Error validating user from jwt token");
                return false;
            }
        }
    }
}
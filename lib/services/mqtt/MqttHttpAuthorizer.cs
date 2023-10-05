using lib.models.mqtt;
using System.Threading.Tasks;
using System;
using System.Linq;
using lib.models.configuration;

namespace lib.services.mqtt
{
    public interface IMqttHttpAuthorizer
    {
        Task<EqmxAuthorizeResponse> Authorize(string username, string topic, string action);
    }

    public class MqttHttpAuthorizer : IMqttHttpAuthorizer
    {
        private readonly IWorkspaceService _workspaceService;
        private readonly AppConfiguration _appConfiguration;
        
        public MqttHttpAuthorizer(
            IWorkspaceService workspaceService,
            AppConfiguration appConfiguration
        )
        {
            _workspaceService = workspaceService;
            _appConfiguration = appConfiguration;
        }

        public async Task<EqmxAuthorizeResponse> Authorize(string username, string topic, string action)
        {
            if (IsSuperUser(username)) {
                return new EqmxAuthorizeResponse() {
                    Result = AuthResultOptions.Allow,
                };
            } 
            else if (IsDeviceTopic(topic))
            {
                return AuthorizeDevice(username, topic);
            } 
            else if (IsWorkspaceTopic(topic))
            {
                return await AuthorizeWorkspace(username, topic, action);
            } else {
                return new EqmxAuthorizeResponse() {
                    Result = AuthResultOptions.Deny,
                };
            }
        }

        private bool IsSuperUser(string username)
        {
            return username == _appConfiguration.MQTT.AdminUsername;
        }

        private bool IsWorkspaceTopic(string topic)
        {
            return topic.Contains("workspace");
        }

        private bool IsDeviceTopic(string topic)
        {
            return topic.Contains("device");
        }

        private EqmxAuthorizeResponse AuthorizeDevice(string username, string topic)
        {
            Guid topicDeviceId = MqttTopicManager.GetDeviceIdFromTopic(topic);
            Guid authenticateUser = Guid.Parse(username);
            if (topicDeviceId == authenticateUser)
            {
                return new EqmxAuthorizeResponse() {
                    Result = AuthResultOptions.Allow,
                };
            } else {
                return new EqmxAuthorizeResponse() {
                    Result = AuthResultOptions.Deny,
                };
            }
        }

        private async Task<EqmxAuthorizeResponse> AuthorizeWorkspace(string username, string topic, string action)
        {
            Guid topicWorkspaceId = MqttTopicManager.GetWorkspaceIdFromTopic(topic);
            Guid authenticateUserId = Guid.Parse(username);
            var workspace = await _workspaceService.GetWorkspace(topicWorkspaceId);
            if (workspace == null)
            {
                return new EqmxAuthorizeResponse() {
                    Result = AuthResultOptions.Deny,
                };
            }
            bool isWorkspaceDevice = workspace.Devices.Any(d => d.Id == authenticateUserId);
            if (isWorkspaceDevice) {
                return new EqmxAuthorizeResponse() {
                    Result = AuthResultOptions.Allow,
                };
            }
            bool isWorkspaceUser = workspace.WorkspaceUsers.Any(wu => wu.UserId == authenticateUserId);
            if (isWorkspaceUser) {
                if (action == "publish")
                {  
                    var user = workspace
                                    .WorkspaceUsers
                                    .Where(wu => wu.UserId == authenticateUserId)
                                    .Select(wu => wu.User)
                                    .First();
                   if (_workspaceService.IsWorkspaceEditor(workspace.Id, user))
                   {
                       return new EqmxAuthorizeResponse() {
                           Result = AuthResultOptions.Allow,
                       };
                   } else {
                       return new EqmxAuthorizeResponse() {
                           Result = AuthResultOptions.Deny,
                       };
                   }
                }
                return new EqmxAuthorizeResponse() {
                    Result = AuthResultOptions.Deny,
                };
            } else {
                return new EqmxAuthorizeResponse() {
                    Result = AuthResultOptions.Deny,
                };
            }
        }
        
    }
}
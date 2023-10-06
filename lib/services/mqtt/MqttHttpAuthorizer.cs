using lib.models.mqtt;
using System.Threading.Tasks;
using System;
using System.Linq;
using lib.models.configuration;
using lib.models;
using lib.models.db;
using System.Collections.Generic;

namespace lib.services.mqtt
{
    public interface IMqttHttpAuthorizer
    {
        Task<EqmxAuthorizeResponse> Authorize(string username, string topic, string action);
    }

    public class MqttHttpAuthorizer : IMqttHttpAuthorizer
    {
        private readonly CvopsDbContext _dbContext;
        private readonly AppConfiguration _appConfiguration;
        
        public MqttHttpAuthorizer(
            CvopsDbContext dbContext,
            AppConfiguration appConfiguration
        )
        {
            _dbContext = dbContext;
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
            var workspace = await GetWorkspace(topicWorkspaceId);
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
                   if (IsWorkspaceEditor(workspace, user))
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

        private async Task<Workspace> GetWorkspace(Guid workspaceId)
        {
            var workspace = await _dbContext.Workspaces.FindAsync(workspaceId);
            if (workspace != null)
            {
                return workspace;
            }
            throw new Exception($"Workspace {workspaceId} not found");
        }

        private static readonly WorkspaceUserRole[] _editorRoles = new WorkspaceUserRole[] {
            WorkspaceUserRole.Owner, 
            WorkspaceUserRole.Editor,
        };

        private bool IsWorkspaceEditor(Workspace workspace, User user)
        {
            var workspaceUser = workspace.WorkspaceUsers.Where(wu => wu.UserId == user.Id).First();
            return _editorRoles.Contains(workspaceUser.WorkspaceUserRole);
        }
    }
}
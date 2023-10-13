using System.Threading.Tasks;
using lib.services;
using lib.models.db;
using Serilog;
using lib.services.factories;
using System.Threading.Channels;
using lib.models.mqtt;
using System.Text.Json;
using lib.extensions;
using System;

namespace mqtt.workers
{
    public class WorkspaceEventWorker : ChannelListener<PlatformEvent>
    {
        private readonly IStorageService _storageService;
        private readonly IScopedServiceFactory<IWorkspaceService> _workspaceServiceFactory;
        private readonly ChannelWriter<MqttPublishMessage> _publishMessageWriter;
        private readonly IScopedServiceFactory<IDeploymentService> _deploymentServiceFactory;

        public WorkspaceEventWorker(
            IStorageService storageService, 
            ILogger logger,
            IScopedServiceFactory<IWorkspaceService> workspaceServiceFactory,
            ChannelWriter<MqttPublishMessage> publishMessageWriter,
            IScopedServiceFactory<IDeploymentService> deploymentServiceFactory
        ) : base(logger)
        {
            _storageService = storageService;
            _workspaceServiceFactory = workspaceServiceFactory;
            _publishMessageWriter = publishMessageWriter;
            _deploymentServiceFactory = deploymentServiceFactory;
        }

        public override async Task HandleMessage(PlatformEvent platformEvent)
        {
            switch (platformEvent.EventType) {
                case PlatformEventTypes.WorkspaceCreated:
                    await CreateWorkspace(platformEvent);
                    break;
                case PlatformEventTypes.WorkspaceDeleted:
                    await DeleteWorkspace(platformEvent);
                    break;
                case PlatformEventTypes.WorkspaceDetailsRequest:
                    await ReplyWithWorkspaceDetails(platformEvent);
                    break;
                case PlatformEventTypes.DeploymentUpdated:
                    await HandleUpdatedDeployment(platformEvent);
                    break;
                default:
                    break;
            }
        }

        private async Task CreateWorkspace(PlatformEvent platformEvent)
        {
            await _storageService.CreateBucket(platformEvent.WorkspaceId.ToString());
        }

        private async Task DeleteWorkspace(PlatformEvent platformEvent)
        {
            await _storageService.DeleteBucket(platformEvent.WorkspaceId.ToString());
        }

        private async Task ReplyWithWorkspaceDetails(PlatformEvent platformEvent)
        {
            if (platformEvent.ResponseTopic == null) {
                throw new Exception("Cannot Reply to Workspace Details Request without a response topic");
            }
            using (var workspaceService = _workspaceServiceFactory.Create())
            {
                var workspace = await workspaceService.GetWorkspace(platformEvent.WorkspaceId);              
                var payload = new PlatformEvent() {
                    EventType = PlatformEventTypes.WorkspaceDetailsResponse,
                    EventData = JsonSerializer.SerializeToDocument(workspace.ToDto(), LocalJsonOptions.DefaultOptions),
                    WorkspaceId = workspace.Id,
                    DeviceId = platformEvent.DeviceId,
                    ResponseTopic = platformEvent.ResponseTopic
                };
                var publishMessage = new MqttPublishMessage() {
                    Topic = platformEvent.ResponseTopic,
                    Payload = payload
                };
                await _publishMessageWriter.WriteAsync(publishMessage);
            }
        }

        private async Task HandleUpdatedDeployment(PlatformEvent platformEvent)
        {
            using (var deploymentService = _deploymentServiceFactory.Create())
            {
                await deploymentService.HandleDeploymentUpdateEvent(platformEvent);
            }
        }
    }
}
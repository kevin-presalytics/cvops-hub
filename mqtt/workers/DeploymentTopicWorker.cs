using lib.services;
using lib.services.mqtt;
using lib.models.mqtt;
using Serilog;
using lib.services.factories;
using System.Threading.Tasks;
using System.Threading.Channels;
using MQTTnet;
using dto = lib.models.dto;
using lib.extensions;

namespace mqtt.workers
{
    public class DeploymentTopicListener : MqttTopicListener
    {
        private readonly IScopedServiceFactory<IDeploymentService> _deploymentServiceFactory;
        private readonly ChannelWriter<MqttPublishMessage> _publishMessageWriter;

        public DeploymentTopicListener(
            ILogger logger,
            ChannelWriter<MqttSubscriptionMessage> subscriptionWriter,
            IScopedServiceFactory<IDeploymentService> deploymentServiceFactory,
            ChannelWriter<MqttPublishMessage> publishMessageWriter
        ) : base(logger, subscriptionWriter)
        {
            _deploymentServiceFactory = deploymentServiceFactory;
            _publishMessageWriter = publishMessageWriter;
        }

        public override string TopicFilter => "$share/g/workspace/+/deployments";

        public override async Task HandleMessage(MqttApplicationMessage message)
        {
            dto.DeploymentMessage deploymentMessage = message.AsMqttPayload<dto.DeploymentMessage>();
            switch (deploymentMessage.MessageType)
            {
                case dto.DeploymentMessageTypes.Created:
                    dto.DeploymentCreatedMessage created = message.AsMqttPayload<dto.DeploymentCreatedMessage>();
                    await HandleDeploymentCreated(created.Payload);
                    break;
                case dto.DeploymentMessageTypes.Updated:
                    dto.Deployment updated = message.AsMqttPayload<dto.DeploymentUpdatedMessage>().Payload;
                    await HandleDeploymentUpdated(updated);
                    break;
                case dto.DeploymentMessageTypes.DeviceStatus:
                    dto.DeviceDeploymentStatus deviceStatus = message.AsMqttPayload<dto.DeploymentDeviceStatusUpdatedMessage>().Payload;
                    await HandleDeviceStatusUpdate(deviceStatus);
                    break;
                case dto.DeploymentMessageTypes.Deleted:
                    dto.DeploymentDeletedPayload deleted = message.AsMqttPayload<dto.DeploymentDeletedMessage>().Payload;
                    await HandleDeploymentDeleted(deleted);
                    break;
                default:
                    _logger.Warning($"Unknown deployment message type: {deploymentMessage.MessageType}");
                    break;
            }
        }

        private async Task HandleDeploymentCreated(dto.DeploymentCreatedPayload payload)
        {
            using (IDeploymentService deploymentService = _deploymentServiceFactory.Create())
            {
                var deployment = await deploymentService.CreateDeployment(payload);
            }
        }

        private async Task HandleDeploymentUpdated(dto.Deployment dto)
        {
            using (IDeploymentService deploymentService = _deploymentServiceFactory.Create())
            {
                var deployment = await deploymentService.GetDeployment(dto.Id);
                deployment.DevicesStatus = dto.DevicesStatus;
                deployment.Status = dto.Status;
                deployment.ModelMetadata = dto.ModelMetadata;
                deployment.ModelSource = dto.ModelSource;
                deployment.BucketName = dto.BucketName;
                deployment.ObjectName = dto.ObjectName;
                deployment.ModelType = dto.ModelType;
                deployment.DevicesStatus = dto.DevicesStatus;
                await deploymentService.UpdateDeployment(deployment);
            }
        }

        public async Task HandleDeploymentDeleted(dto.DeploymentDeletedPayload payload)
        {
            using (IDeploymentService deploymentService = _deploymentServiceFactory.Create())
            {
                await deploymentService.DeleteDeployment(payload.DeploymentId);
            }
        }

        private async Task HandleDeviceStatusUpdate(dto.DeviceDeploymentStatus deviceStatus)
        {
            using (IDeploymentService deploymentService = _deploymentServiceFactory.Create())
            {
                var updatedDeployment = await deploymentService.UpdateDeviceStatus(deviceStatus);
            }
        }
    }
}
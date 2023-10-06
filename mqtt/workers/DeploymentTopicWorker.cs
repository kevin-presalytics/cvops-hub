using lib.services;
using lib.services.mqtt;
using lib.models.mqtt;
using Serilog;
using lib.services.factories;
using System.Threading.Tasks;
using System.Threading.Channels;
using MQTTnet;
using dto = lib.models.dto;

namespace mqtt.workers
{
    public class DeploymentTopicListener : MqttTopicListener
    {
        private readonly IScopedServiceFactory<IDeploymentService> _deploymentServiceFactory;

        public DeploymentTopicListener(
            ILogger logger,
            ChannelWriter<MqttSubscriptionMessage> subscriptionWriter,
            IScopedServiceFactory<IDeploymentService> deploymentServiceFactory
        ) : base(logger, subscriptionWriter)
        {
            _deploymentServiceFactory = deploymentServiceFactory;
        }

        public override string TopicFilter => "$share/g/workspace/+/deployments";

        public override async Task HandleMessage(MqttApplicationMessage message)
        {
            dto.DeploymentMessage deploymentMessage = message.AsMqttPayload<dto.DeploymentMessage>();
            switch (deploymentMessage.MessageType)
            {
                case dto.DeploymentMessageTypes.Created:
                    dto.DeploymentCreatedPayload created = (dto.DeploymentCreatedPayload)deploymentMessage.Payload;
                    await HandleDeploymentCreated(created);
                    break;
                case dto.DeploymentMessageTypes.Updated:
                    dto.Deployment updated = (dto.Deployment)deploymentMessage.Payload;
                    await HandleDeploymentUpdated(updated);
                    break;
                case dto.DeploymentMessageTypes.DeviceStatus:
                    dto.DeviceDeploymentStatus deviceStatus = (dto.DeviceDeploymentStatus)deploymentMessage.Payload;
                    await HandleDeviceStatusUpdate(deviceStatus);
                    break;
                case dto.DeploymentMessageTypes.Deleted:
                    dto.DeploymentDeletedPayload deleted = (dto.DeploymentDeletedPayload)deploymentMessage.Payload;
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
                await deploymentService.CreateDeployment(payload);
            }
        }

        private async Task HandleDeploymentUpdated(dto.Deployment deploymentDTO)
        {
            using (IDeploymentService deploymentService = _deploymentServiceFactory.Create())
            {
                var deployment = await deploymentService.GetDeployment(deploymentDTO.Id);
                deployment.DevicesStatus = deploymentDTO.DevicesStatus ;
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
                await deploymentService.UpdateDeviceStatus(deviceStatus);
            }
        }
    }
}
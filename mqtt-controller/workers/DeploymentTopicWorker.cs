using lib.services;
using lib.services.mqtt;
using lib.models.mqtt;
using Serilog;
using lib.services.factories;
using System.Threading.Tasks;
using System.Threading.Channels;
using MQTTnet;
using lib.models.dto;

namespace mqtt_controller.workers
{
    public class DeploymentTopicListener : MqttTopicListener
    {
        IScopedServiceFactory<IDeploymentService> _deploymentServiceFactory;

        public DeploymentTopicListener(
            ILogger logger,
            ChannelWriter<MqttSubscriptionMessage> subscriptionWriter,
            IScopedServiceFactory<IDeploymentService> deploymentServiceFactory
        ) : base(logger, subscriptionWriter)
        {
            _deploymentServiceFactory = deploymentServiceFactory;
        }

        public override string TopicFilter => "$share/g/workspace/+/deployments/#";

        public override async Task HandleMessage(MqttApplicationMessage message)
        {
            DeploymentMessage deploymentMessage = message.AsMqttPayload<DeploymentMessage>();
            switch (deploymentMessage.MessageType)
            {
                case DeploymentMessageTypes.Created:
                    DeploymentCreatedPayload created = (DeploymentCreatedPayload)deploymentMessage.Payload;
                    await HandleDeploymentCreated(created);
                    break;
                case DeploymentMessageTypes.Updated:
                    DeploymentUpdatedPayload updated = (DeploymentUpdatedPayload)deploymentMessage.Payload;
                    await HandleDeploymentUpdated(updated);
                    break;
                case DeploymentMessageTypes.Deleted:
                    DeploymentDeletedPayload deleted = (DeploymentDeletedPayload)deploymentMessage.Payload;
                    await HandleDeploymentDeleted(deleted);
                    break;
                default:
                    _logger.Warning($"Unknown deployment message type: {deploymentMessage.MessageType}");
                    break;
            }
        }

        private async Task HandleDeploymentCreated(DeploymentCreatedPayload payload)
        {
            using (IDeploymentService deploymentService = _deploymentServiceFactory.Create())
            {
                await deploymentService.CreateDeployment(payload);
            }
        }

        private async Task HandleDeploymentUpdated(DeploymentUpdatedPayload payload)
        {
            using (IDeploymentService deploymentService = _deploymentServiceFactory.Create())
            {
                await deploymentService.UpdateDeployment(payload.Deployment);
            }
        }

        public async Task HandleDeploymentDeleted(DeploymentDeletedPayload payload)
        {
            using (IDeploymentService deploymentService = _deploymentServiceFactory.Create())
            {
                await deploymentService.DeleteDeployment(payload.DeploymentId);
            }
        }
    }
}
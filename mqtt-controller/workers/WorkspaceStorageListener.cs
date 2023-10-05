using lib.services.mqtt;
using MQTTnet;
using System.Threading.Tasks;
using System.Threading.Channels;
using Serilog;
using lib.services.factories;
using lib.models.mqtt;
using lib.services;
using System;
using lib.models.dto;

namespace mqtt_controller.workers
{
    public class WorkspaceStorageListener : MqttTopicListener
    {
        private readonly IScopedServiceFactory<IStorageService> _storageServiceFactory;
        private readonly ChannelWriter<MqttPublishMessage> _publishMessageWriter;
        public WorkspaceStorageListener(
            ILogger logger,
            ChannelWriter<MqttSubscriptionMessage> subscriptionWriter,
            IScopedServiceFactory<IStorageService> storageServiceFactory,
            ChannelWriter<MqttPublishMessage> publishMessageWriter

        ) : base(logger, subscriptionWriter)
        {
            _storageServiceFactory = storageServiceFactory;
            _publishMessageWriter = publishMessageWriter;
        }

        public override string TopicFilter => "$share/g/workspace/+/storage";

        public override async Task HandleMessage(MqttApplicationMessage message)
        {
            var storageMessage = message.AsMqttPayload<WorkspaceStorageMessage>();
            Guid workspaceId = MqttTopicManager.GetWorkspaceIdFromTopic(message.Topic);
            string responseTopic = storageMessage.ResponseTopic;
            switch (storageMessage.Payload.Type)
            {
                case StorageMessageType.PutUrlRequest:
                    string putUrl = await CreatePutUrl(workspaceId);
                    await SendUrlResponse(putUrl, responseTopic, StorageMessageType.PutUrlResponse);
                    break;
                case lib.models.dto.StorageMessageType.GetUrlRequest:
                    string getUrl = await CreateGetUrl(workspaceId);
                    await SendUrlResponse(getUrl, responseTopic, StorageMessageType.GetUrlResponse);
                    break;
                default:
                    _logger.Warning($"Unknown storage message type: {storageMessage.Payload.Type}");
                    break;
            }

        }

        private async Task SendUrlResponse(string url, string responseTopic, StorageMessageType messageType)
        {
            var payload = new WorkspaceStoragePayload() {
                Type = messageType,
                Url = url
            };
            var message = new WorkspaceStorageMessage() { Payload = payload};
            var publishMessage = new MqttPublishMessage() {
                Topic = responseTopic,
                Payload = message
            };
            await _publishMessageWriter.WriteAsync(publishMessage);
        }

        private async Task<string> CreatePutUrl(Guid workspaceId)
        {
            using (var storageService = _storageServiceFactory.Create())
            {
                return await storageService.CreatePresignedPutUrl(workspaceId.ToString(), Guid.NewGuid().ToString());
            }
        }
        private async Task<string> CreateGetUrl(Guid workspaceId)
        {
            using (var storageService = _storageServiceFactory.Create())
            {
                return await storageService.CreatePresignedGetUrl(workspaceId.ToString(), Guid.NewGuid().ToString());
            }
        }



    }
}
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

namespace mqtt.workers
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
                    string objectName = storageMessage.Payload.ObjectName ?? Guid.NewGuid().ToString();
                    await CreateBucketIfNotExists(objectName, workspaceId);
                    string putUrl = await CreatePutUrl(objectName, workspaceId);
                    await SendUrlResponse(putUrl, responseTopic, StorageMessageType.PutUrlResponse);
                    break;
                case StorageMessageType.GetUrlRequest:
                    if (storageMessage.Payload.ObjectName == null) {
                        throw new Exception("Object name is required for GetUrlRequest");
                    }
                    string getUrl = await CreateGetUrl(storageMessage.Payload.ObjectName, workspaceId);
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

        private async Task<string> CreatePutUrl(string objectName, Guid workspaceId)
        {
            using (var storageService = _storageServiceFactory.Create())
            {
                await storageService.CreateObjectIfNotExists(objectName, workspaceId.ToString());
                return await storageService.CreatePresignedPutUrl(objectName, workspaceId.ToString());
            }
        }
        private async Task<string> CreateGetUrl(string objectName, Guid workspaceId)
        {
            using (var storageService = _storageServiceFactory.Create())
            {
                var exists = await storageService.ObjectExists(objectName, workspaceId.ToString());
                if (!exists)  {
                    throw new Exception($"Object {objectName} does not exist in bucket {workspaceId}");
                }  
                return await storageService.CreatePresignedGetUrl(workspaceId.ToString(), Guid.NewGuid().ToString());
            }
        }

        private async Task CreateBucketIfNotExists(string objectName, Guid workspaceId)
        {
            using (var storageService = _storageServiceFactory.Create())
            {
                await storageService.CreateObjectIfNotExists(objectName, workspaceId.ToString());
            }
        }
    }
}
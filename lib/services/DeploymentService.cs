using lib.models.db;
using dto = lib.models.dto;
using lib.models;
using System.Threading.Tasks;
using System;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using System.Linq;
using lib.extensions;
using System.Threading.Channels;
using lib.models.mqtt;
using System.Collections.Generic;


namespace lib.services
{
    public interface IDeploymentService : IDisposable
    {
        Task<Deployment> GetDeployment(Guid DeploymentId);
        Task<Deployment> CreateDeployment(dto.DeploymentCreatedPayload payload); 
        Task<Deployment> UpdateDeployment(Deployment updatedDeployment);
        Task DeleteDeployment(Guid DeploymentId);
        Task<Deployment> UpdateDeviceStatus(dto.DeviceDeploymentStatus deviceStatus);
        Task HandleDeploymentUpdateEvent(PlatformEvent platformEvent);
    }

    public class DeploymentService : IDeploymentService
    {
        private readonly CvopsDbContext _context;
        private readonly ChannelWriter<MqttPublishMessage> _publishMessageWriter;

        public DeploymentService(
            IDbContextFactory<CvopsDbContext> contextFactory,
            ChannelWriter<MqttPublishMessage> publishMessageWriter
        ) {
            _context = contextFactory.CreateDbContext();
            _publishMessageWriter = publishMessageWriter;
        }

        public async Task<Deployment> CreateDeployment(dto.DeploymentCreatedPayload payload)
        {
            Guid Id = Guid.NewGuid();
            Deployment deployment = new Deployment {
                Id = Id,
                DeploymentInitiatorId = payload.DeploymentInitiatorId,
                DeploymentInitiatorType = payload.DeploymentInitiatorType,
                ModelSource = payload.ModelSource,
                WorkspaceId = payload.WorkspaceId,
                BucketName = payload.BucketName ?? payload.WorkspaceId.ToString(),
                ObjectName = $"models/{Id}",
                ModelType = payload.ModelType,
                Status = DeploymentStatus.Created,
                DevicesStatus = CreateDevicesStatus(payload.DeviceIds, Id),
                ModelMetadata = payload.ModelMetadata ?? JsonDocument.Parse("{}"),         
            };
            _context.Deployments.Add(deployment);
            _context.SaveChanges();
            await EmitPlatformEvent(deployment, PlatformEventTypes.DeploymentCreated);
            return deployment;
        }

        private JsonDocument CreateDevicesStatus(List<Guid> deviceIds, Guid deploymentId)
        {
            var devices = deviceIds.Select(deviceId => new dto.DeviceDeploymentStatus {
                DeploymentId = deploymentId,
                DeviceId = deviceId,
                Status = dto.DeviceDeploymentStatusTypes.None,
            }).ToList();
            var status = new dto.DevicesStatus() { Devices = devices};
            return JsonDocument.Parse(JsonSerializer.Serialize(status, LocalJsonOptions.DefaultOptions));
        }

        public async Task<Deployment> GetDeployment(Guid DeploymentId)
        {
            Deployment? deployment = await _context.Deployments.FindAsync(DeploymentId);
            if (deployment == null) {
                throw new Exception($"Deployment {DeploymentId} not found");
            }
            return deployment;
        }

        public async Task<Deployment> UpdateDeployment(Deployment deployment)
        {
            _context.Deployments.Update(deployment);
            await _context.SaveChangesAsync();
            await EmitPlatformEvent(deployment);
            return deployment;
        }

        public async Task DeleteDeployment(Guid deploymentId)
        {
            var deployment = await GetDeployment(deploymentId);
            _context.Deployments.Remove(deployment);
            await _context.SaveChangesAsync();
            await EmitPlatformEvent(deployment, PlatformEventTypes.DeploymentDeleted);
        }
        
        public async Task<Deployment> UpdateDeviceStatus(dto.DeviceDeploymentStatus deviceStatus)
        {
            Deployment deployment = await GetDeployment(deviceStatus.DeploymentId);
            if (deployment == null) {
                throw new Exception($"Deployment {deviceStatus.DeploymentId} not found");
            }
            string entry = deployment.DevicesStatus.RootElement.ToString() ?? "{}";
            dto.DevicesStatus? devicesStatus = JsonSerializer.Deserialize<dto.DevicesStatus>(entry);
            if (devicesStatus == null) {
                throw new Exception($"Unable to deserialize devices status for deployment {deviceStatus.DeploymentId}");
            }
            var tmpDevicesStatus = devicesStatus.Devices.Where(d => d.DeviceId != deviceStatus.DeviceId).ToList();
            tmpDevicesStatus.Add(deviceStatus);
            devicesStatus.Devices = tmpDevicesStatus;
            deployment.DevicesStatus = JsonDocument.Parse(JsonSerializer.Serialize(devicesStatus));
            await _context.SaveChangesAsync();
            await EmitPlatformEvent(deployment);
            return deployment;
        }

        private async Task EmitPlatformEvent(Deployment deployment, PlatformEventTypes eventType = PlatformEventTypes.DeploymentUpdated)
        {
            
            var platformEvent = new PlatformEvent {
                WorkspaceId = deployment.WorkspaceId,
                DeviceId = deployment.DeploymentInitiatorType == EditorTypes.Device ? deployment.DeploymentInitiatorId : null,
                UserId = deployment.DeploymentInitiatorType == EditorTypes.User ? deployment.DeploymentInitiatorId : null,
                EventType = eventType,
                EventData = GetEventData(deployment),
            };

            MqttPublishMessage message = new MqttPublishMessage() {
                Topic = $"events/workspace/{deployment.WorkspaceId}",
                Payload = platformEvent,
            };

            await _publishMessageWriter.WriteAsync(message);
        }

        private JsonDocument GetEventData(Deployment deployment)
        {
            return JsonDocument.Parse(JsonSerializer.Serialize(deployment.ToDto(), LocalJsonOptions.DefaultOptions));
        }

        private async Task ValidateModel(Deployment deployment)
        {
            // TODO: create model validation service
            // 1. Vaidate onnx model
            // 2. Ensure onnx model can run on taret devices

            deployment.Status = DeploymentStatus.ModelValidated;
            await UpdateDeployment(deployment);
            await EmitPlatformEvent(deployment);
        }

        private async Task InitiateDeploymentToDevices(Deployment deployment)
        {
            deployment.Status = DeploymentStatus.ModelDeploying;
            await UpdateDeployment(deployment);
            await EmitPlatformEvent(deployment);
        }



        public async Task HandleDeploymentUpdateEvent(PlatformEvent platformEvent)
        {
            if (platformEvent.EventType != PlatformEventTypes.DeploymentUpdated) {
                throw new Exception($"Cannot handle deployment updates for event type {platformEvent.EventType}");
            }
            var dto = JsonSerializer.Deserialize<dto.Deployment>(platformEvent.EventData, LocalJsonOptions.DefaultOptions);
            if (dto == null) {
                throw new Exception("Unable to deserialize deployment data from platform event");
            }
            var deployment = await GetDeployment(dto.Id);
            switch (deployment.Status) {
                case DeploymentStatus.ModelUploaded:
                    await ValidateModel(deployment);
                    break;
                case DeploymentStatus.ModelValidated:
                    await InitiateDeploymentToDevices(deployment);
                    break;
                default:
                    break;
            }
            
        }

        public void Dispose()
        {
            _context.Dispose();
        }
        
    }
}
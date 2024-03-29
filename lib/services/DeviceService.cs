using lib.models.db;
using System.Threading.Tasks;
using dto = lib.models.dto;
using lib.models;
using lib.models.configuration;
using lib.services.auth;
using Microsoft.EntityFrameworkCore;
using System;
using System.Text.Json;
using lib.models.exceptions;
using lib.extensions;

namespace lib.services
{
    public interface IDeviceService : IDisposable
    {
        Task<Device> GetDevice(Guid deviceId);
        Task<Device> UpdateDevice(Device device);
        Task<dto.NewDevice> CreateNewDevice(Workspace workspace);
        Task DeleteDevice(Device device);
        Task<bool> Authenticate(Guid deviceId, string secretKey);
    
    }

    public class DeviceService : IDeviceService
    {
        private readonly CvopsDbContext _context;
        private readonly AppConfiguration _configuration;
        private readonly IDeviceKeyGenerator _keyGenerator;
        private readonly IDeviceKeyVerifier _keyVerifier;

        public DeviceService(            
            IDbContextFactory<CvopsDbContext> contextFactory, 
            IDeviceKeyGenerator keyGenerator, 
            AppConfiguration configuration,
            IDeviceKeyVerifier keyVerifier
        )
        {
            _context = contextFactory.CreateDbContext();
            _configuration = configuration;
            _keyGenerator = keyGenerator;
            _keyVerifier =  keyVerifier;
        }

        public async Task<Device> GetDevice(Guid deviceId)
        {
            return await _context.Devices.FirstOrDefaultAsync(d => d.Id == deviceId) ?? throw new DeviceNotFoundException();
        }

        public async Task<Device> UpdateDevice(Device device)
        {
            _context.Devices.Update(device);
            await _context.SaveChangesAsync();
            return device;
        }

        public async Task<dto.NewDevice> CreateNewDevice(Workspace workspace)
        {
            dto.SecureDeviceCredentials _key = _keyGenerator.GenerateKey();
            if (workspace == null)
                throw new WorkspaceNotFoundException();
            Device device = new Device() {
                Salt = _key.Salt,
                Hash = _key.Hash,
                DeviceInfo = JsonDocument.Parse("{}"),
                Workspace = workspace,
                Name = "Unregistered Device",
                Description = String.Empty
            };
            workspace.Devices.Add(device);
            _context.Workspaces.Update(workspace);
            await _context.Devices.AddAsync(device);
            await _context.SaveChangesAsync();
            return new dto.NewDevice() {
                Id = device.Id,
                SecretKey = _key.Key,
                MqttUri = _configuration.GetPublicMqttConnectionUrl()
            };

        }

        public async Task DeleteDevice(Device device)
        {
            _context.Devices.Remove(device);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> Authenticate(Guid deviceId, string secretKey)
        {
            Device device = await GetDevice(deviceId);
            return _keyVerifier.Verify(secretKey, device.Hash, device.Salt);
        }

        void IDisposable.Dispose()
        {
            _context.Dispose();
        }
    }
}
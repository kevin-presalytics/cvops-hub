using lib.models.db;
using System.Threading.Tasks;
using dto = lib.models.dto;
using lib.models;
using lib.models.configuration;
using lib.services.auth;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using System;
using System.Text.Json;
using lib.models.exceptions;
using lib.extensions;

namespace lib.services
{
    public interface IDeviceService
    {
        Task<Device> GetDevice(Guid deviceId);
        Task<Device> UpdateDevice(Device device);
        Task<dto.NewDevice> CreateNewDevice(Workspace workspace);
        Task DeleteDevice(Device device);
        Task<bool> Authenticate(Guid deviceId, string secretKey);
    
    }

    public class DeviceService : IDeviceService
    {
        CvopsDbContext _context;
        AppConfiguration _configuration;
        IDeviceKeyGenerator _keyGenerator;
        IDeviceKeyVerifier _keyVerifier;

        public DeviceService(            
            CvopsDbContext context, 
            IDeviceKeyGenerator keyGenerator, 
            AppConfiguration configuration,
            IDeviceKeyVerifier keyVerifier
        )
        {
            _context = context;
            _configuration = configuration;
            _keyGenerator = keyGenerator;
            _keyVerifier =  keyVerifier;
        }

        public async Task<Device> GetDevice(Guid deviceId)
        {
            Device? device = await _context.Devices.FirstOrDefaultAsync(d => d.Id == deviceId);
            if (device == null)
                throw new DeviceNotFoundException();
            return device;
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
    }
}
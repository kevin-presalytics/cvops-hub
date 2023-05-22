using lib.models.db;
using System.Threading.Tasks;

namespace lib.services
{
    public interface IDeviceService
    {
        Task<Device> GetDeviceById(string deviceId);
        Task<Device> UpdateDevice(Device device);
    
    }
    public class DeviceService : IDeviceService
    {
        public DeviceService()
        {
        }

        public async Task<Device> GetDeviceById(string deviceId)
        {
            throw new System.NotImplementedException();
        }

        public async Task<Device> UpdateDevice(Device device)
        {
            throw new System.NotImplementedException();
        }
    }
}
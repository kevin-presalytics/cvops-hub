using System;
using Microsoft.Extensions.DependencyInjection;

namespace lib.services.factories
{
    public interface IDeviceServiceFactory
    {
        IDeviceService Create();
    }
    public class DeviceServiceFactory : IDeviceServiceFactory
    {
        private readonly IServiceProvider _serviceProvider;
        public DeviceServiceFactory(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }
        public IDeviceService Create()
        {
            using (var scope = _serviceProvider.CreateScope()) 
            {
                return scope.ServiceProvider.GetRequiredService<IDeviceService>();
            }
        }
    }
}
using System;
using Microsoft.Extensions.DependencyInjection;

namespace lib.services.factories
{
    public interface IScopedServiceFactory<T> where T : IDisposable
    {
        T Create();
    }
    public class ScopedServiceFactory<T> : IScopedServiceFactory<T> where T : IDisposable
    {
        private readonly IServiceProvider _serviceProvider;
        public ScopedServiceFactory(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }
        public T Create()
        {
            return _serviceProvider.GetRequiredService<T>();
        }
    }
}
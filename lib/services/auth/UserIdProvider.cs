using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Http;
using System;
using System.Linq;

namespace lib.services.auth
{

    public interface IUserIdProvider
    {
        Guid GetUserId();
        void SetUserId(Guid userId);
    }

    public class ScopedUserIdProvider : IUserIdProvider
    {
        private IHttpContextAccessor httpContextAccessor;
        private Guid? ScopedUserId { get; set;}
        public ScopedUserIdProvider(IHttpContextAccessor contextAccessor)
        {
            httpContextAccessor = contextAccessor;
            ScopedUserId = null;
        }

        public Guid GetUserId()
        {
            if (ScopedUserId != null)
            {
                return (Guid)ScopedUserId;
            } else {
                if (httpContextAccessor?.HttpContext != null)
                {
                    if (httpContextAccessor?.HttpContext.User.Claims.FirstOrDefault(i => i.Type == "https://api.presalytics.io/api_user_id") != null) {
                        ScopedUserId = Guid.NewGuid(); //Guid.Parse(httpContextAccessor?.HttpContext.User.Claims.First(i => i.Type == "https://api.presalytics.io/api_user_id").Value);
                        return (Guid)ScopedUserId;                        
                    }
                }
                return Guid.Empty;
            }
        }

        public void SetUserId(Guid userId)
        {
            ScopedUserId = userId;
        }
    }

    public static class UserIdProviderExtensions
    {
        public static IServiceScope CreateUserServiceScope(this IServiceProvider serviceProvider, Guid? userId = null)
        {
            if (userId == null)
            {
                IUserIdProvider userIdProvider = serviceProvider.GetRequiredService<IUserIdProvider>();
                userId = userIdProvider.GetUserId(); 
            }
            IServiceScope scope = serviceProvider.CreateScope();
            IUserIdProvider newUserIdProvider = scope.ServiceProvider.GetRequiredService<IUserIdProvider>();
            newUserIdProvider.SetUserId((Guid)userId);
            return scope;

        }
    }

    public class SystemUserIdProvider : IUserIdProvider
    {
        public Guid GetUserId()
        {
            return Guid.Empty;
        }

        public void SetUserId(Guid userId)
        {
            // Do nothing
        }
    }
    
}
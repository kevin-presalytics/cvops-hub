using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Http;
using System;
using System.Linq;
using lib.models.configuration;

namespace lib.services.auth
{

    public interface IUserIdProvider
    {
        Guid? GetUserId();
        void SetUserId(Guid? userId);
    }

    public class ScopedUserIdProvider : IUserIdProvider
    {
        private IHttpContextAccessor _httpContextAccessor;
        private Guid? _scopedUserId { get; set;}
        private string _userIdJwtClaim;
        public ScopedUserIdProvider(IHttpContextAccessor contextAccessor, AppConfiguration appConfig)
        {
            _httpContextAccessor = contextAccessor;
            _scopedUserId = null;
            _userIdJwtClaim = appConfig.Auth.UserIdJwtClaim;
        }

        public Guid? GetUserId()
        {
            if (_scopedUserId != null  && _scopedUserId != Guid.Empty)
            {
                return _scopedUserId;
            } else {
                if (_httpContextAccessor?.HttpContext != null)
                {
                    if (_httpContextAccessor?.HttpContext.User.Claims.FirstOrDefault(i => i.Type == _userIdJwtClaim) != null) {
                        #pragma warning disable CS8604, CS8629
                        _scopedUserId = Guid.Parse(_httpContextAccessor?.HttpContext.User.Claims.First(i => i.Type == _userIdJwtClaim).Value);
                        if (_scopedUserId != Guid.Empty) return _scopedUserId; else return null;    
                        #pragma warning restore CS8604, CS8629
                    }
                }
                return null;
            }
        }

        public void SetUserId(Guid? userId)
        {
            _scopedUserId = userId;
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
            newUserIdProvider.SetUserId((Guid?)userId);
            return scope;

        }
    }    
}
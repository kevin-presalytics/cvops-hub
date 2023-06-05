using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Http;
using System;
using lib.models.configuration;
using lib.middleware;

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
                    #pragma warning disable CS8600
                    IRequestUserFeature userFeature = _httpContextAccessor.HttpContext.Features.Get<IRequestUserFeature>();
                    #pragma warning restore CS8600
                    if (userFeature == null || userFeature.User == null) return null;
                    _scopedUserId = userFeature.User.Id;
                    return userFeature.User.Id;
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
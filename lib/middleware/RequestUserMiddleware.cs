using lib.services;
using lib.models.db;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Builder;

namespace lib.middleware
{
    public class RequestUserMiddleware
    {
        private readonly RequestDelegate _next;

        public RequestUserMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context, IUserService userService)
        {
            string authorization = context.Request.Headers["Authorization"];
            string jwtToken = authorization.Split(" ")[1];
            var user = await userService.GetOrCreateUser(context.Request.Headers["Authorization"]);
            context.Features.Set<IRequestUserFeature>(new RequestUserFeature(user));
            await _next(context);
        }
    }

    public interface IRequestUserFeature
    {
        User User { get; }
    }

    public class RequestUserFeature : IRequestUserFeature
    {
        public User User { get; } = default!;

        public RequestUserFeature(User user)
        {
            User = user;
        }
    }

    public static class RequestUserMiddlewareExtensions
    {
        public static IApplicationBuilder UseRequestUserMiddleware(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<RequestUserMiddleware>();
        }
    }
}
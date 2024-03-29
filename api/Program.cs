using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using lib.models;
using lib.extensions;
using lib.services;
using lib.services.auth;
using lib.models.configuration;
using Serilog;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using api.controllers.options;
using lib.middleware;
using lib.services.factories;

namespace api
{
    public class Program
    {
        public static void Main(string[] args)
        {
            // Setup
            // Requires Environment Variable: DEBUG=true
            DebuggerSetup.WaitForDebugger();
            var builder = WebApplication.CreateBuilder(args);

            // Configuration
            AppConfiguration appConfig = builder.AddAppConfiguration();
            ILogger logger = builder.AddSerilogLogger(appConfig);

            // Framework Services
            builder.Services.AddControllers(
                options =>
                {
                    options.Conventions.Add(new RouteTokenTransformerConvention(new SlugifyUrls()));
                }
            ).ConfigureJson();
            builder.Services.AddCVOpsAuth(appConfig, logger);
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();
            builder.Services.AddHttpContextAccessor();
            builder.Services.AddHttpClient();
            builder.Services.ConfigureJson();
            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowAll", builder =>
                {
                    builder.AllowAnyOrigin()
                        .AllowAnyMethod()
                        .AllowAnyHeader();
                });
            });
            builder.WebHost.ConfigureKestrel(options =>
            {
                options.ListenAnyIP(appConfig.Hub.Api.Port);
            });

            // MQTT Management Services
            builder.Services.AddHubMQTTClient();

            // MQTT Topic Listeners

            
            // Service Layer
            builder.Services.AddTransient<IUserService, UserService>();
            builder.Services.AddTransient<IUserJwtTokenReader, UserJwtTokenReader>();
            builder.Services.AddSingleton<IUserIdProvider, ScopedUserIdProvider>();            
            builder.Services.AddSingleton<IDeviceKeyGenerator, DeviceKeyGenerator>();
            builder.Services.AddTransient<IDeviceKeyVerifier, DeviceKeyVerifier>();
            builder.Services.AddTransient<IInviteUserService, InviteUserService>();
            builder.Services.AddTransient<IWorkspaceService, WorkspaceService>();
            builder.Services.AddTransient<IDeviceService, DeviceService>();


            // Service Factories
            builder.Services.AddSingleton<IScopedServiceFactory<IDeviceService>, ScopedServiceFactory<IDeviceService>>();
            builder.Services.AddSingleton<IScopedServiceFactory<IUserService>, ScopedServiceFactory<IUserService>>();

            
            // Model Layer
            builder.Services.AddDbContextFactory<CvopsDbContext>(options => {   
                options.UseLazyLoadingProxies();
                options.UseNpgsql(appConfig.GetPostgresqlConnectionString());
            });
            // Build DI Container
            var app = builder.Build();

            // Configure the HTTP request pipeline.

            app.UseSerilogRequestLogging();
            app.UseExceptionHandler("/error");
        
            app.UseSwagger();
            app.UseSwaggerUI();
            app.UseAuthentication();
            app.UseAuthorization();
            
            app.UseRequestUserMiddleware();

            app.MapControllers();

            app.Run();

        }

    }


}


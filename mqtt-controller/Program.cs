using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Threading.Tasks;
using lib.extensions;
using lib.models;
using lib.services;
using lib.services.mqtt;
using lib.services.auth;
using Serilog;
using mqtt_controller.workers;
using lib.services.mqtt.listeners;
using lib.services.factories;

namespace mqtt_controller
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            // Setup
            // Requires Environment Variable: DEBUG=true
            DebuggerSetup.WaitForDebugger();
            var builder = WebApplication.CreateBuilder(args);
            
            // App Configuration
            var appConfig = builder.AddAppConfiguration();
            ILogger logger = builder.AddSerilogLogger(appConfig);
            logger.Information("Starting MQTT Controller...");
            logger.Information("Adding MQTT Controller services...");

            // Framework Services
            builder.Services.AddControllers().ConfigureJson();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddHttpContextAccessor();
            builder.Services.ConfigureJson();
            builder.Services.AddCVOpsAuth(appConfig, logger);
            builder.WebHost.ConfigureKestrel(options =>
            {
                options.ListenAnyIP(appConfig.Hub.MqttController.Port);
            });

            // MQTT Management Services
            builder.Services.AddHostedService<MqttAdminSetupWorker>();
            builder.Services.AddScoped<IMqttHttpAuthenticator, MqttHttpAuthenticator>();
            builder.Services.AddMQTTAdmin(appConfig);
            builder.Services.AddHubMQTTClient();

            // MQTT Topic Listeners
            builder.Services.AddDeviceDataTopicListener();
            builder.Services.AddPlatformEventTopicListener();

            // Platform Event Handling Threads
            builder.Services.AddHostedService<DeviceRegistrationWorker>();
            builder.Services.AddHostedService<UserEventsWorker>();
            builder.Services.AddHostedService<WorkspaceEventWorker>();
            
            // Service Layer
            builder.Services.AddSingleton<IUserIdProvider, ScopedUserIdProvider>();
            builder.Services.AddSingleton<IUserNotificationService, UserNotificationService>();
            builder.Services.AddTransient<IDeviceKeyVerifier, DeviceKeyVerifier>();
            builder.Services.AddTransient<IDeviceKeyGenerator, DeviceKeyGenerator>();
            builder.Services.AddTransient<IUserService, UserService>();
            builder.Services.AddTransient<IUserJwtTokenReader, UserJwtTokenReader>();
            builder.Services.AddTransient<IInviteUserService, InviteUserService>();
            builder.Services.AddTransient<IWorkspaceService, WorkspaceService>();
            builder.Services.AddTransient<IDeviceService, DeviceService>();
            builder.Services.AddTransient<IUserService, UserService>();
            builder.Services.AddTransient<IPlatformEventService, PlatformEventService>();
            builder.Services.AddTransient<IInferenceResultService, InferenceResultService>();

            // Service Factories to support scoped and Transient services from Singletons / Background Services
            builder.Services.AddSingleton<IScopedServiceFactory<IDeviceService>, ScopedServiceFactory<IDeviceService>>();
            builder.Services.AddSingleton<IScopedServiceFactory<IUserService>, ScopedServiceFactory<IUserService>>();

            // Model Layer
            builder.Services.AddDbContextFactory<CvopsDbContext>(options => {   
                options.UseLazyLoadingProxies();
                options.UseNpgsql(appConfig.GetPostgresqlConnectionString());
            });
            // Builds DI Container
            var app = builder.Build();

            app.UseSerilogRequestLogging();

            app.MapControllers();

            logger.Information("Launching MQTT Controller...");
            await app.RunAsync();
        }
    }
}


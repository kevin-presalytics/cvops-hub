using Microsoft.Extensions.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Threading.Tasks;
using lib.extensions;
using lib.models;
using lib.services;
using lib.services.mqtt;
using lib.services.auth;
using Serilog;
using mqtt.workers;
using lib.services.mqtt.listeners;
using lib.services.factories;
using lib.models.configuration;

namespace mqtt
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            // Setup
            // Requires Environment Variable: DEBUG=true
            DebuggerSetup.WaitForDebugger();

            
            IHostBuilder builder = Host.CreateDefaultBuilder(args);
            var appConfig = builder.AddAppConfiguration();
            var logger = builder.AddSerilogLogger(appConfig);

            builder.ConfigureServices(services =>
                {
                    // App ConFiguration
                    services.AddSingleton<AppConfiguration>(appConfig);
                    services.AddSingleton<ILogger>(logger);

                    logger.Information("Starting MQTT Controller...");

                    // .NET Framework Services
                    services.AddHttpContextAccessor();
                    services.ConfigureJson();
                    services.AddCVOpsAuth(appConfig, logger);

                    // MQTT Management Services
                    services.AddHubMQTTClient();

                    // MQTT Topic Listeners
                    services.AddDeviceDataTopicListener();
                    services.AddPlatformEventTopicListener();

                    // Platform Event Handling Threads
                    services.AddHostedService<DeviceRegistrationWorker>();
                    services.AddHostedService<UserEventsWorker>();
                    services.AddHostedService<WorkspaceEventWorker>();
                    services.AddHostedService<DeploymentTopicListener>();
                    services.AddHostedService<WorkspaceStorageListener>();


                    // Service Layer
                    services.AddSingleton<IUserIdProvider, ScopedUserIdProvider>();
                    services.AddSingleton<IUserNotificationService, UserNotificationService>();
                    services.AddTransient<IDeviceKeyVerifier, DeviceKeyVerifier>();
                    services.AddTransient<IDeviceKeyGenerator, DeviceKeyGenerator>();
                    services.AddTransient<IUserService, UserService>();
                    services.AddTransient<IUserJwtTokenReader, UserJwtTokenReader>();
                    services.AddTransient<IInviteUserService, InviteUserService>();
                    services.AddTransient<IWorkspaceService, WorkspaceService>();
                    services.AddTransient<IDeviceService, DeviceService>();
                    services.AddTransient<IUserService, UserService>();
                    services.AddTransient<IDeploymentService, DeploymentService>();
                    services.AddTransient<IStorageService, MinioStorageService>();
                    services.AddTransient<IPlatformEventService, PlatformEventService>();
                    services.AddTransient<IInferenceResultService, InferenceResultService>();
                    services.AddTransient<IWorkspaceService, WorkspaceService>();
                    
                    // Service Factories to support scoped and Transient services from Singletons / Background Services
                    services.AddSingleton<IScopedServiceFactory<IDeviceService>, ScopedServiceFactory<IDeviceService>>();
                    services.AddSingleton<IScopedServiceFactory<IUserService>, ScopedServiceFactory<IUserService>>();
                    services.AddSingleton<IScopedServiceFactory<IDeploymentService>, ScopedServiceFactory<IDeploymentService>>();
                    services.AddSingleton<IScopedServiceFactory<IWorkspaceService>, ScopedServiceFactory<IWorkspaceService>>();
                    services.AddSingleton<IScopedServiceFactory<IStorageService>, ScopedServiceFactory<IStorageService>>();

                    // Model Layer
                    services.AddDbContextFactory<CvopsDbContext>(options => {   
                        options.UseLazyLoadingProxies();
                        options.UseNpgsql(appConfig.GetPostgresqlConnectionString());
                    });

                    logger.Information("Launching MQTT Controller...");
                });

            IHost host = builder.Build();
            
            await host.RunAsync();
        }
    }
}




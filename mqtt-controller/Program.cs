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
            builder.Services.AddHostedService<ControllerMqttClientWorker>();
            builder.Services.AddSingleton<IMqttTopicRouter, MqttTopicRouter>();
            builder.Services.AddScoped<IMqttHttpAuthenticator, MqttHttpAuthenticator>();
            builder.Services.AddMQTTAdmin(appConfig);
            builder.Services.AddHubMQTTClient();

            // MQTT Topic Listeners
            builder.Services.AddTransient<IMqttTopicListener, UserLoginTopicListener>();
            builder.Services.AddSingleton<IMqttTopicListener, DeviceDataTopicListener>();
            builder.Services.AddHostedService<DeviceRegisteredWorker>();
            builder.Services.AddHostedService<DeviceUnregisteredWorker>();
            
            // Service Layer
            builder.Services.AddSingleton<IUserIdProvider, ScopedUserIdProvider>();
            builder.Services.AddTransient<IDeviceKeyVerifier, DeviceKeyVerifier>();
            builder.Services.AddTransient<IDeviceKeyGenerator, DeviceKeyGenerator>();
            builder.Services.AddTransient<IUserService, UserService>();
            builder.Services.AddTransient<IUserJwtTokenReader, UserJwtTokenReader>();
            builder.Services.AddTransient<IInviteUserService, InviteUserService>();
            builder.Services.AddTransient<IWorkspaceService, WorkspaceService>();
            builder.Services.AddTransient<IDeviceService, DeviceService>();
            builder.Services.AddTransient<IUserService, UserService>();


            // Model Layer
            builder.Services.AddDbContext<CvopsDbContext>(options => options.UseNpgsql(appConfig.GetPostgresqlConnectionString()));

            var app = builder.Build();

            app.UseSerilogRequestLogging();

            app.MapControllers();

            logger.Information("Launching MQTT Controller...");
            logger.Debug("Launching MQTT Controller on in Debug mode.");
            await app.RunAsync();
        }
    }
}


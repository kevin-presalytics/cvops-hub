using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Threading.Tasks;
using lib.extensions;
using lib.models;
using lib.services.mqtt;
using lib.services.auth;
using lib.services.mqtt.queue;
using Serilog;
using mqtt_controller.workers;
using lib.services.mqtt.workers;
using lib.services;
using lib.models.mqtt;

namespace mqtt_controller
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            DebuggerSetup.WaitForDebugger();
            var builder = WebApplication.CreateBuilder(args);
            

            var appConfig = builder.AddAppConfiguration();
            ILogger logger = builder.AddSerilogLogger(appConfig);
            logger.Information("Starting MQTT Controller...");
            logger.Information("Adding MQTT Controller services...");

            
            builder.Services.AddControllers().ConfigureJson();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddHostedService<MqttAdminSetupWorker>();
            builder.Services.AddHostedService<ControllerMqttClientWorker>();


            builder.Services.AddDbContext<CvopsDbContext>(options => options.UseNpgsql(appConfig.GetPostgresqlConnectionString()));
            builder.Services.AddMQTTAdmin(appConfig);
            builder.Services.AddHubMQTTClient();
            builder.Services.AddSingleton<IUserIdProvider, ScopedUserIdProvider>();
            builder.Services.AddTransient<IQueueBroker, QueueBroker>();
            builder.Services.AddTransient<IDeviceKeyVerifier, DeviceKeyVerifier>();
            builder.Services.AddTransient<IDeviceKeyGenerator, DeviceKeyGenerator>();
            builder.Services.AddScoped<IMqttHttpAuthenticator, MqttHttpAuthenticator>();
            builder.Services.AddTransient<IUserService, UserService>();
            builder.Services.AddTransient<IUserJwtTokenReader, UserJwtTokenReader>();
            builder.Services.AddCVOpsAuth(appConfig, logger);
            builder.Services.AddHttpContextAccessor();
            builder.Services.ConfigureJson();

            // Add Queues
            builder.Services.AddSingleton<IMqttTopicQueue<UserLoginPayload>, MqttTopicQueue<UserLoginPayload>>();

            // Add Queue Workers
            builder.Services.AddHostedService<UserLoginTopicWorker>();



            builder.WebHost.ConfigureKestrel(options =>
            {
                options.ListenAnyIP(appConfig.Hub.MqttController.Port);
            });


            var app = builder.Build();

            app.UseSerilogRequestLogging();

            app.MapControllers();

            logger.Information("Launching MQTT Controller...");
            logger.Debug("Launching MQTT Controller on in Debug mode.");
            await app.RunAsync();
        }
    }
}


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

namespace mqtt_controller
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            

            var appConfig = builder.AddAppConfiguration();
            ILogger logger = builder.AddSerilogLogger(appConfig);
            logger.Information("Starting MQTT Controller...");
            logger.Information("Adding MQTT Controller services...");

            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddHostedService<MqttAdminSetupWorker>();

            builder.Services.AddDbContext<CvopsDbContext>(options => options.UseNpgsql(appConfig.GetPostgresqlConnectionString()));
            builder.Services.AddMQTTAdmin(appConfig);
            builder.Services.AddHubMQTTClient();
            builder.Services.AddSingleton<IUserIdProvider, SystemUserIdProvider>();
            builder.Services.AddTransient<IQueueBroker, QueueBroker>();
            builder.Services.AddTransient<IDeviceKeyVerifier, DeviceKeyVerifier>();
            builder.Services.AddTransient<IDeviceKeyGenerator, DeviceKeyGenerator>();


            builder.WebHost.ConfigureKestrel(options =>
            {
                options.ListenAnyIP(appConfig.Hub.MqttController.Port);
            });


            var app = builder.Build();

            app.MapControllers();

            logger.Information("Launching MQTT Controller...");
            await app.RunAsync();
        }
    }
}


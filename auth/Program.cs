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


namespace auth
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
            logger.Information("Starting Hub HTTP Auth Service...");

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
            builder.Services.AddScoped<IMqttHttpAuthenticator, MqttHttpAuthenticator>();
            builder.Services.AddScoped<IMqttHttpAuthorizer, MqttHttpAuthorizer>();
            builder.Services.AddHostedService<MqttAdminSetupWorker>();
            builder.Services.AddMQTTAdmin(appConfig);
            
            // Service Layer
            builder.Services.AddSingleton<IUserIdProvider, ScopedUserIdProvider>();
            builder.Services.AddTransient<IDeviceKeyVerifier, DeviceKeyVerifier>();
            builder.Services.AddTransient<IDeviceKeyGenerator, DeviceKeyGenerator>();
            builder.Services.AddTransient<IUserService, UserService>();
            builder.Services.AddTransient<IUserJwtTokenReader, UserJwtTokenReader>();
            builder.Services.AddTransient<IInviteUserService, InviteUserService>();
            builder.Services.AddTransient<IUserService, UserService>();


            // Model Layer
            builder.Services.AddDbContextFactory<CvopsDbContext>(options => {   
                options.UseLazyLoadingProxies();
                options.UseNpgsql(appConfig.GetPostgresqlConnectionString());
            });
            // Builds DI Container
            var app = builder.Build();

            app.UseSerilogRequestLogging();

            app.MapControllers();

            logger.Information("Internal Auth Service application build.  Launching...");
            await app.RunAsync();
        }
    }
}


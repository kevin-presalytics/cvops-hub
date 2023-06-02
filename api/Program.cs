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
using lib.services.mqtt;

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
            builder.WebHost.ConfigureKestrel(options =>
            {
                options.ListenAnyIP(appConfig.Hub.Api.Port);
            });

            // MQTT Management Services
            builder.Services.AddHubMQTTClient();
            builder.Services.AddSingleton<IMqttTopicRouter, MqttTopicRouter>();
            builder.Services.AddHostedService<MqttClientWorker>();

            // MQTT Topic Listeners

            
            // Service Layer
            builder.Services.AddTransient<IUserService, UserService>();
            builder.Services.AddTransient<IUserJwtTokenReader, UserJwtTokenReader>();
            builder.Services.AddSingleton<IUserIdProvider, ScopedUserIdProvider>();            
            builder.Services.AddSingleton<IDeviceKeyGenerator, DeviceKeyGenerator>();
            
            // Model Layer
            builder.Services.AddDbContext<CvopsDbContext>(options => options.UseNpgsql(appConfig.GetPostgresqlConnectionString()));

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


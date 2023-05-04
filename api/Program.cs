using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
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
using lib.services.mqtt.workers;
using lib.services.mqtt.queue;

namespace api
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            AppConfiguration config = builder.AddAppConfiguration();
            ILogger logger = builder.AddSerilogLogger(config);
            builder.Services.AddControllers(
                options =>
                {
                    options.Conventions.Add(new RouteTokenTransformerConvention(new SlugifyUrls()));
                }
            ).ConfigureJson();

            builder.Services.AddHostedService<MqttClientWorker>();
            builder.Services.AddHubMQTTClient();
            builder.Services.AddSingleton<IQueueBroker, QueueBroker>();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();
            builder.Services.AddDbContext<CvopsDbContext>(options => options.UseNpgsql(config.GetPostgresqlConnectionString()));
            builder.Services.AddSingleton<IDeviceKeyGenerator, DeviceKeyGenerator>();
            builder.Services.AddHttpContextAccessor();
            builder.Services.AddSingleton<IUserIdProvider, ScopedUserIdProvider>();
            builder.Services.AddHttpClient();
            builder.Services.AddTransient<IUserService, UserService>();
            builder.Services.AddTransient<IUserJwtTokenReader, UserJwtTokenReader>();
            builder.Services.AddCVOpsAuth(config, logger);
            builder.Services.ConfigureJson();

            builder.WebHost.ConfigureKestrel(options =>
            {
                options.ListenAnyIP(config.Hub.Api.Port);
            });

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


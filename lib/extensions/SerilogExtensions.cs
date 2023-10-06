using Serilog;
using Serilog.Events;
using Serilog.Formatting.Json;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Hosting;
using System;
using lib.models.configuration;


namespace lib.extensions
{
    public static class SerilogLogger
    {
        public static Serilog.ILogger AddSerilogLogger(this WebApplicationBuilder builder, AppConfiguration config)
        {
            Log.Logger = GetLoggerConfiguration(config);
            AppDomain.CurrentDomain.ProcessExit += (s, e) => Log.CloseAndFlush();
            builder.Services.AddSingleton<Serilog.ILogger>(Log.Logger);
            builder.Host.UseSerilog(Log.Logger);
            return Log.Logger;
        }

        public static Serilog.ILogger AddSerilogLogger(this IHostBuilder builder, AppConfiguration config)
        {
            Log.Logger = GetLoggerConfiguration(config);
            AppDomain.CurrentDomain.ProcessExit += (s, e) => Log.CloseAndFlush();
            builder.ConfigureLogging(logging =>
            {
                logging.ClearProviders();
                logging.AddSerilog(Log.Logger);
            });
            return Log.Logger;
        }

        public static Serilog.ILogger GetLoggerConfiguration(AppConfiguration config)
        {
             var logConfig = new LoggerConfiguration()
                .MinimumLevel.Is(config.Logging.Level)
                .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
                .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
                .MinimumLevel.Override("Microsoft.EntityFrameworkCore", LogEventLevel.Warning)
                .MinimumLevel.Override("System", LogEventLevel.Information)
                // .MinimumLevel.Override("Microsoft.EntityFrameworkCore.Database.Command", LogEventLevel.Information)
                .Enrich.FromLogContext();
                
            if (config.Logging.Format == "json")
            {
                logConfig.WriteTo.Console(new JsonFormatter(), config.Logging.Level);
            }
            else
            {
                logConfig.WriteTo.Console(config.Logging.Level);
            }

            return logConfig.CreateLogger();
        }

        public static Serilog.ILogger GetSerilogLogger(this IServiceCollection services)
        {
            IServiceProvider _provider = services.BuildServiceProvider();
            return _provider.GetRequiredService<Serilog.ILogger>();
        }
    }
}
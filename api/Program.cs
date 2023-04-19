using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using lib.models;
using lib.extensions;
using lib.services.auth;
using lib.models.configuration;
using Serilog;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using api.controllers.options;

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
            );

            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();
            builder.Services.AddDbContext<CvopsDbContext>(options => options.UseNpgsql(config.GetPostgresqlConnectionString()));
            builder.Services.AddSingleton<IDeviceKeyGenerator, DeviceKeyGenerator>();
            builder.Services.AddHttpContextAccessor();
            builder.Services.AddSingleton<IUserIdProvider, SystemUserIdProvider>();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
        
            app.UseSwagger();
            app.UseSwaggerUI();    

            // app.UseAuthorization();

            app.MapControllers();

            app.Run();

        }

    }


}


using db_migrations;
using lib.models.configuration;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Metadata;
using System;
using lib.services.auth;
using Utility.Extensions.Configuration.Yaml;
using lib.models.configuration;
using lib.extensions;

namespace db_migrations
{
    public static class Services
    {
        public static IServiceProvider ConfigureServices()
        {
            var config = new ConfigurationBuilder()
                .AddYamlFile("appsettings.default.yaml", optional: false, reloadOnChange: true)
                .AddYamlFile("appsettings.yaml", optional: true, reloadOnChange: true)
                .AddYamlFile("appsettings.local.yaml", optional: true, reloadOnChange: true)
                .Build();

            AppConfiguration appConfiguration = new AppConfiguration();
            config.Bind(appConfiguration);

            IServiceCollection services = new ServiceCollection();
            services.AddDbContext<MigrationsDbContext>(options => options.UseNpgsql(appConfiguration.GetPostgresqlConnectionString()));
            services.AddSingleton<AppConfiguration>(appConfiguration);
            services.AddSingleton<IUserIdProvider, SystemUserIdProvider>();

            IServiceProvider serviceProvider = services.BuildServiceProvider();
            return serviceProvider;
        }
    }
}
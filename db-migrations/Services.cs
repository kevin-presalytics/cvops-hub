using lib.models.configuration;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;
using lib.services.auth;
using Utility.Extensions.Configuration.Yaml;
using lib.extensions;

namespace db_migrations
{
    public static class Services
    {
        public static IServiceProvider ConfigureServices()
        {
            AppConfiguration appConfiguration = new ConfigurationManager().Configure();
            IServiceCollection services = new ServiceCollection();
            services.AddDbContext<MigrationsDbContext>(options => options.UseNpgsql(appConfiguration.GetPostgresqlConnectionString()));
            services.AddSingleton<AppConfiguration>(appConfiguration);
            services.AddSingleton<IUserIdProvider, ScopedUserIdProvider>();
            services.AddHttpContextAccessor();

            IServiceProvider serviceProvider = services.BuildServiceProvider();
            return serviceProvider;
        }
    }
}
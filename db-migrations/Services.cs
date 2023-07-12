using lib.models.configuration;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using lib.services.auth;
using lib.extensions;
using lib.models;

namespace db_migrations
{
    public static class Services
    {
        public static IServiceProvider ConfigureServices()
        {
            AppConfiguration appConfiguration = new ConfigurationManager().Configure();
            IServiceCollection services = new ServiceCollection();
            services.AddDbContextFactory<CvopsDbContext>(options => {
                options.UseNpgsql(appConfiguration.GetPostgresqlConnectionString(), b => b.MigrationsAssembly("db-migrations"));
            });
            services.AddSingleton<AppConfiguration>(appConfiguration);
            services.AddSingleton<IUserIdProvider, ScopedUserIdProvider>();
            services.AddHttpContextAccessor();
            IServiceProvider serviceProvider = services.BuildServiceProvider();
            return serviceProvider;
        }
    }
}
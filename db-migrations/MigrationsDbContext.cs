using lib.models;
using lib.models.configuration;
using lib.services.auth;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.DependencyInjection;

namespace db_migrations
{
    public class MigrationsDbContext : CvopsDbContext
    {
        public MigrationsDbContext(AppConfiguration configuration, IUserIdProvider userIdProvider) : base(configuration, userIdProvider)
        {
        }
        
    }

    public class MigrationsDbContextFactory : IDesignTimeDbContextFactory<MigrationsDbContext>
    {
        public MigrationsDbContext CreateDbContext(string[] args)
        {
            var serviceProvider = Services.ConfigureServices();
            return serviceProvider.GetRequiredService<MigrationsDbContext>();
        }
}
}
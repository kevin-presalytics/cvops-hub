using lib.models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.DependencyInjection;

namespace db_migrations
{

    public class MigrationsDbContextFactory : IDesignTimeDbContextFactory<CvopsDbContext>
    {
        public CvopsDbContext CreateDbContext(string[] args)
        {
            var serviceProvider = Services.ConfigureServices();
            var dbContextFactory = serviceProvider.GetRequiredService<IDbContextFactory<CvopsDbContext>>();
            return dbContextFactory.CreateDbContext();
        }
}
}
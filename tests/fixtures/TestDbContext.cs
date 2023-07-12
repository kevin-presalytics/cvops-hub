using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using lib.models;
using lib.services.auth;
using lib.models.configuration;
using Moq;

namespace tests.fixtures
{
    // // TestDbContext class
    // public class TestDbContext : CvopsDbContext
    // {
    //     // TestDbContext constructor
    //     public TestDbContext(DbContextOptions<CvopsDbContext> options) : base(options) { }

    //     protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    //     {
    //         string instanceId = Guid.NewGuid().ToString();
    //         optionsBuilder.UseInMemoryDatabase(instanceId);
    //     }

    //     protected override void OnModelCreating(ModelBuilder modelBuilder)
    //     {
    //         base.OnModelCreating(modelBuilder);
    //         modelBuilder.Entity<Device>()
    //             .Property(d => d.DeviceInfo)
    //             .HasConversion(
    //                 v => v.Stringify(),
    //                 v => JsonDocument.Parse(v, new JsonDocumentOptions())
    //             );
    //     }
    // }

    public class DatabaseUnitTest : IDisposable
    {
        protected CvopsDbContext _context;
        protected IDbContextFactory<CvopsDbContext> _contextFactory;

        public DatabaseUnitTest()
        {
            IServiceCollection serviceCollection = new ServiceCollection();

            serviceCollection.AddDbContextFactory<CvopsDbContext>(options =>
            {
                var instance = Guid.NewGuid().ToString();
                options.UseLazyLoadingProxies();
                options.UseInMemoryDatabase(instance);
            });
            serviceCollection.AddSingleton<AppConfiguration>(new AppConfiguration());
            var mockUserIdProvider = new Mock<IUserIdProvider>();
            
            serviceCollection.AddSingleton<IUserIdProvider>(mockUserIdProvider.Object);
            serviceCollection.AddHttpContextAccessor();
            var mockConfiguration = new Mock<AppConfiguration>();
            serviceCollection.AddSingleton<AppConfiguration>(mockConfiguration.Object);
            IServiceProvider serviceProvider = serviceCollection.BuildServiceProvider();
            _contextFactory = serviceProvider.GetRequiredService<IDbContextFactory<CvopsDbContext>>();
            _context = _contextFactory.CreateDbContext();
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }
};
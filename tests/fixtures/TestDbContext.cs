using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using lib.models;
using lib.services.auth;
using lib.models.configuration;
using lib.models.db;
using System.Text.Json;
using Moq;
using lib.extensions;

namespace tests.fixtures
{
    // TestDbContext class
    public class TestDbContext : CvopsDbContext
    {
        // TestDbContext constructor
        public TestDbContext(AppConfiguration configuration, IUserIdProvider userIdProvider) : base(configuration, userIdProvider) { }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            string instanceId = Guid.NewGuid().ToString();
            optionsBuilder.UseInMemoryDatabase(instanceId);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<Device>()
                .Property(d => d.DeviceInfo)
                .HasConversion(
                    v => v.Stringify(),
                    v => JsonDocument.Parse(v, new JsonDocumentOptions())
                );
        }
    }

    public class DatabaseUnitTest : IDisposable
    {
        protected TestDbContext _context;

        public DatabaseUnitTest()
        {
            IServiceCollection serviceCollection = new ServiceCollection();

            serviceCollection.AddDbContext<TestDbContext>();
            serviceCollection.AddSingleton<AppConfiguration>(new AppConfiguration());
            var mockUserIdProvider = new Mock<IUserIdProvider>();
            
            serviceCollection.AddSingleton<IUserIdProvider>(mockUserIdProvider.Object);
            serviceCollection.AddHttpContextAccessor();
            var mockConfiguration = new Mock<AppConfiguration>();
            serviceCollection.AddSingleton<AppConfiguration>(mockConfiguration.Object);
            IServiceProvider serviceProvider = serviceCollection.BuildServiceProvider();

            _context = serviceProvider.GetRequiredService<TestDbContext>();
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }
};
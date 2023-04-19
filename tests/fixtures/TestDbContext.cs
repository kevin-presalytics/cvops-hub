using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using lib.models;
using lib.services.auth;

namespace tests.fixtures
{
    // TestDbContext class
    public class TestDbContext : CvopsDbContext
    {
        // TestDbContext constructor
        public TestDbContext(IConfiguration configuration, IUserIdProvider userIdProvider) : base(configuration, userIdProvider) { }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            string instanceId = Guid.NewGuid().ToString();
            optionsBuilder.UseInMemoryDatabase(instanceId);
        }
    }

    public class DatabaseUnitTest : IDisposable
    {
        protected TestDbContext _context;

        public DatabaseUnitTest()
        {
            IServiceCollection serviceCollection = new ServiceCollection();

            serviceCollection.AddDbContext<TestDbContext>();
            serviceCollection.AddSingleton<IConfiguration>(new ConfigurationBuilder().Build());
            serviceCollection.AddTransient<IUserIdProvider, SystemUserIdProvider>();
            IServiceProvider serviceProvider = serviceCollection.BuildServiceProvider();

            _context = serviceProvider.GetRequiredService<TestDbContext>();
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }
};
// DBContext for cvops database (EntityFramework) for models int eh lib/models/db folder

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using lib.models.db;
using lib.services.auth;

namespace lib.models
{

    public class CvopsDbContext : DbContext
    {
        private IConfiguration _configuration { get; }
        private IUserIdProvider _userIdProvider {get; }


        public DbSet<Device> Devices => Set<Device>();

        public CvopsDbContext(IConfiguration configuration, IUserIdProvider userIdProvider)
        {
            _configuration = configuration;
            _userIdProvider = userIdProvider;
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseNpgsql(_configuration.GetConnectionString("cvops"));
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Device>().HasKey(e => e.Id);
        }

        public override int SaveChanges()
        {
            AddTimeStamps();
            return base.SaveChanges();
        }

        public override async Task<int> SaveChangesAsync(System.Threading.CancellationToken cancellationToken = default)
        {
            AddTimeStamps();
            return await base.SaveChangesAsync();
        }

        private void AddTimeStamps()
        {
            IEnumerable<Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry> entities = ChangeTracker.Entries().Where(x => x.Entity is BaseEntity && (x.State == EntityState.Added || x.State == EntityState.Modified));

            Guid userId = _userIdProvider.GetUserId();
            foreach (Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry entity in entities)
            {
                if (entity.State == EntityState.Added)
                {
                    ((BaseEntity)entity.Entity).DateCreated = DateTime.UtcNow;
                    ((BaseEntity)entity.Entity).UserCreated = userId; // Need better username method
                }
                ((BaseEntity)entity.Entity).DateModified = DateTime.UtcNow;
                ((BaseEntity)entity.Entity).UserModified = userId;
            }
        }
    }
}
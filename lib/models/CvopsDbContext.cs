// DBContext for cvops database (EntityFramework) for models int eh lib/models/db folder

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using lib.models.db;
using lib.services.auth;
using lib.models.configuration;
using lib.extensions;

namespace lib.models
{

    public class CvopsDbContext : DbContext
    {
        private AppConfiguration _configuration { get; }
        private IUserIdProvider _userIdProvider {get; }


        public DbSet<Device> Devices => Set<Device>();

        public DbSet<User> Users => Set<User>();

        public DbSet<WorkspaceUser> WorkspaceUsers => Set<WorkspaceUser>();

        public DbSet<Workspace> Workspaces => Set<Workspace>();

        public DbSet<InferenceResult> InferenceResults => Set<InferenceResult>();
        public DbSet<PlatformEvent> PlatformEvents => Set<PlatformEvent>();



        public CvopsDbContext(AppConfiguration configuration, IUserIdProvider userIdProvider)
        {
            _configuration = configuration;
            _userIdProvider = userIdProvider;
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseLazyLoadingProxies();
            optionsBuilder.UseNpgsql(_configuration.GetPostgresqlConnectionString());
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Configure Regular Tables
             List<Type> _entityTypes = modelBuilder.Model.GetEntityTypes()
                .Where(ent => ent.ClrType.IsSubclassOf(typeof(BaseEntity)))
                .Select(ent => ent.ClrType)
                .ToList();


            foreach (Type ent in _entityTypes)
            {

                Type generic = typeof(EntitySchemaConfigurator<>).MakeGenericType(ent);
                #pragma warning disable CS8600
                dynamic _genericConfigurator = Activator.CreateInstance(generic);
                #pragma warning restore CS8600
                modelBuilder.ApplyConfiguration(_genericConfigurator);
            }

            List<Microsoft.EntityFrameworkCore.Metadata.IMutableEntityType> dbEntities = modelBuilder.Model.GetEntityTypes()
                                .Where(e => e.ClrType.IsSubclassOf(typeof(BaseEntity)))
                                .ToList();

            foreach (Microsoft.EntityFrameworkCore.Metadata.IMutableEntityType entity in dbEntities)
            {
                modelBuilder.Entity(entity.Name)
                    .Property("Id")
                    .ValueGeneratedOnAdd();

                modelBuilder.Entity(entity.Name)
                    .HasKey("Id");
                
                modelBuilder.Entity(entity.Name)
                    .Property("CreatedBy")
                    .HasConversion(new EnumToStringConverter<EditorTypes>());

                modelBuilder.Entity(entity.Name)
                    .Property("ModifiedBy")
                    .HasConversion(new EnumToStringConverter<EditorTypes>());

                var tableName = entity.GetTableName();
                #pragma warning disable CS8604
                var storeObjectIdentifier = StoreObjectIdentifier.Table(tableName, null);
                #pragma warning restore CS8604

                // Replace column names            
                foreach (Microsoft.EntityFrameworkCore.Metadata.IMutableProperty property in entity.GetProperties())
                {
                    property.SetColumnName(((IProperty)property).GetColumnName(storeObjectIdentifier).ToSnakeCase());

                }

                foreach (Microsoft.EntityFrameworkCore.Metadata.IMutableKey key in entity.GetKeys())
                {
                    #pragma warning disable CS8600
                    key.SetName((string)key.GetName().ToSnakeCase());
                    #pragma warning restore CS8600
                }

                foreach (Microsoft.EntityFrameworkCore.Metadata.IMutableForeignKey key in entity.GetForeignKeys())
                {
                    key.SetConstraintName(key.GetConstraintName().ToSnakeCase());
                }

                foreach (Microsoft.EntityFrameworkCore.Metadata.IMutableIndex index in entity.GetIndexes())
                {
                    index.SetDatabaseName(index.GetDatabaseName().ToSnakeCase());
                }
            }

            // User
            // Indexes
            modelBuilder.Entity<User>()
                .HasIndex(u => u.Email)
                .IsUnique();
            
            modelBuilder.Entity<User>()
                .HasIndex(u => u.JwtSubject)
                .IsUnique();

            modelBuilder.Entity<User>()
                .Property(u => u.Status)
                .HasConversion(new EnumToStringConverter<UserStatus>());
                
            // Device
            modelBuilder.Entity<Device>()
                .HasIndex(d => d.Id)
                .IsUnique();

            modelBuilder.Entity<Device>()
                .HasIndex(d => d.WorkspaceId);
            
            modelBuilder.Entity<Device>()
                .Property(d => d.ActivationStatus)
                .HasConversion(new EnumToStringConverter<DeviceActivationStatus>());

            // Workspace
            modelBuilder.Entity<Workspace>()
                .HasMany(w => w.WorkspaceUsers)
                .WithOne(wu => wu.Workspace);

            modelBuilder.Entity<Workspace>()
                .HasMany(t => t.Devices)
                .WithOne(d => d.Workspace)
                .HasForeignKey(d => d.WorkspaceId);

            // WorkspaceUser
            modelBuilder.Entity<WorkspaceUser>()
                .HasOne(wu => wu.User)
                .WithMany();

            modelBuilder.Entity<WorkspaceUser>()
                .Property(wu => wu.WorkspaceUserRole)
                .HasConversion(new EnumToStringConverter<WorkspaceUserRole>());


            // Configure HyperTables
            // InferenceResult
            modelBuilder.Entity<InferenceResult>()
                .HasIndex(ir => ir.DeviceId);
            modelBuilder.Entity<InferenceResult>()
                .HasIndex(ir => ir.WorkspaceId);
            modelBuilder.Entity<InferenceResult>()
                .HasIndex(ir => ir.ResultType);

            modelBuilder.Entity<InferenceResult>()
                .ToTable<InferenceResult>("inference_results", t => t.ExcludeFromMigrations());

            // PlatformEvent
            modelBuilder.Entity<PlatformEvent>()
                .HasIndex(pe => pe.DeviceId);
            modelBuilder.Entity<PlatformEvent>()
                .HasIndex(pe => pe.WorkspaceId);
            modelBuilder.Entity<PlatformEvent>()
                .HasIndex(pe => pe.EventType);
            modelBuilder.Entity<PlatformEvent>()
                .HasIndex(pe => pe.UserId);
            
            modelBuilder.Entity<InferenceResult>()
                .ToTable<InferenceResult>("platform_events", t => t.ExcludeFromMigrations());

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

            Guid? userId = _userIdProvider.GetUserId();
            EditorTypes editorType = EditorTypes.User;
            if (userId == Guid.Empty || userId == null) editorType = EditorTypes.System;
            
            foreach (Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry entity in entities)
            {
                if (entity.State == EntityState.Added)
                {
                    ((BaseEntity)entity.Entity).DateCreated = DateTime.UtcNow;
                    ((BaseEntity)entity.Entity).UserCreated = userId; // Need better username method
                    ((BaseEntity)entity.Entity).CreatedBy = editorType;
                }
                ((BaseEntity)entity.Entity).DateModified = DateTime.UtcNow;
                ((BaseEntity)entity.Entity).UserModified = userId;
                ((BaseEntity)entity.Entity).ModifiedBy = editorType;
            }
        }
    }

    public class EntitySchemaConfigurator<T> : IEntityTypeConfiguration<T> where T : BaseEntity
    {
        public void Configure(EntityTypeBuilder<T> builder)
        {
            #pragma warning disable CS8600
            string _tableName = builder.Metadata.ClrType.Name.ToSnakeCase();
            // "user" is a reserved table name is postgres
            if (_tableName == "user") _tableName = "cvops_user";
            #pragma warning restore CS8600
            _ = builder.ToTable(_tableName);
        }
    }

}
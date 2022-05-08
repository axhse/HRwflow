using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace HRwflow.Models.Data
{
    public abstract class DatabaseContext<TEntity> : DbContext, IDatabaseContext<TEntity>
        where TEntity : class
    {
        protected DatabaseContext(DbContextOptions options) : base(options)
        {
            // Database.EnsureCreated();
        }

        protected DatabaseContext(string connectionString)
            : base(new DbContextOptions<DatabaseContext<TEntity>>())
        {
            Database.SetConnectionString(connectionString);
            // Database.EnsureCreated();
        }

        public DbSet<TEntity> Items { get; set; }

        public EntityEntry<TEntity> Entry(TEntity entity) => base.Entry(entity);

        protected abstract IEntityTypeConfiguration<TEntity> CreateConfiguration();

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer();
            base.OnConfiguring(optionsBuilder);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfiguration(CreateConfiguration());
            base.OnModelCreating(modelBuilder);
        }
    }

    public class DatabaseSet<TEntity> : DbSet<TEntity>
            where TEntity : class
    { }
}

using Microsoft.EntityFrameworkCore;
using DotNetCleanTemplate.Domain.Entities;

namespace DotNetCleanTemplate.Infrastructure.Persistent
{
    public class AppDbContext : DbContext
    {
        public DbSet<User> Users { get; set; }
        public DbSet<Role> Roles { get; set; }

        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.ApplyConfiguration(new UserConfiguration());
            modelBuilder.ApplyConfiguration(new RoleConfiguration());
            // Конфигурация сущностей будет добавлена отдельно
        }
    }
}

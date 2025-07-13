using DotNetCleanTemplate.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DotNetCleanTemplate.Infrastructure.Persistent
{
    public class UserRoleConfiguration : IEntityTypeConfiguration<UserRole>
    {
        public void Configure(EntityTypeBuilder<UserRole> builder)
        {
            builder.HasKey(ur => ur.Id);
            builder.Property(ur => ur.Id).ValueGeneratedNever();

            builder.HasIndex(ur => new { ur.UserId, ur.RoleId }).IsUnique();
            // Навигационные свойства и связи настраиваются в User/Role
        }
    }
}

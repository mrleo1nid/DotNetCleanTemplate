using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using DotNetCleanTemplate.Domain.Entities;

namespace DotNetCleanTemplate.Infrastructure.Persistent
{
    public class RoleConfiguration : IEntityTypeConfiguration<Role>
    {
        public void Configure(EntityTypeBuilder<Role> builder)
        {
            builder.HasKey(r => r.Id);
            builder.Property(r => r.Id).ValueGeneratedNever();

            builder.OwnsOne(
                r => r.Name,
                n =>
                {
                    n.Property(p => p.Value).IsRequired();
                }
            );

            builder.Property<DateTime>("CreatedAt");
            builder.Property<DateTime>("UpdatedAt");
        }
    }
}

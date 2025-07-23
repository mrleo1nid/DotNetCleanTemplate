using DotNetCleanTemplate.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DotNetCleanTemplate.Infrastructure.Persistent
{
    public class UserConfiguration : IEntityTypeConfiguration<User>
    {
        public void Configure(EntityTypeBuilder<User> builder)
        {
            builder.HasKey(u => u.Id);
            builder.Property(u => u.Id).ValueGeneratedNever();

            builder.OwnsOne(
                u => u.Name,
                n =>
                {
                    n.Property(p => p.Value).IsRequired();
                }
            );
            builder.OwnsOne(
                u => u.Email,
                e =>
                {
                    e.Property(p => p.Value).IsRequired();
                    e.HasIndex(p => p.Value).IsUnique();
                }
            );
            builder.OwnsOne(
                u => u.PasswordHash,
                p =>
                {
                    p.Property(x => x.Value).IsRequired();
                }
            );

            builder.Property<DateTime>("CreatedAt");
            builder.Property<DateTime>("UpdatedAt");

            builder
                .HasMany(u => u.UserRoles)
                .WithOne("User")
                .HasForeignKey("UserId")
                .OnDelete(DeleteBehavior.Cascade);
            builder
                .Navigation(nameof(User.UserRoles))
                .UsePropertyAccessMode(PropertyAccessMode.Field);
        }
    }
}

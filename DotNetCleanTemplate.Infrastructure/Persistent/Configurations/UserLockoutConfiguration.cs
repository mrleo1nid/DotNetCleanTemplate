using DotNetCleanTemplate.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DotNetCleanTemplate.Infrastructure.Persistent.Configurations;

public class UserLockoutConfiguration : IEntityTypeConfiguration<UserLockout>
{
    public void Configure(EntityTypeBuilder<UserLockout> builder)
    {
        builder.ToTable("UserLockouts");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.UserId).IsRequired();

        builder.Property(x => x.LockoutEnd).IsRequired();

        builder.Property(x => x.FailedAttempts).IsRequired().HasDefaultValue(0);

        builder.Property(x => x.CreatedAt).IsRequired();

        builder.Property(x => x.UpdatedAt);

        // Связь с User
        builder
            .HasOne(x => x.User)
            .WithMany()
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // Индексы
        builder.HasIndex(x => x.UserId).IsUnique();

        builder.HasIndex(x => x.LockoutEnd);
    }
}

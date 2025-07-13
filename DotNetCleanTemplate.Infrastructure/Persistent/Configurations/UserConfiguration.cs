using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using DotNetCleanTemplate.Domain.Entities;

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
                .Property<List<Guid>>("_roleIds")
                .HasField("_roleIds")
                .HasConversion(
                    v => string.Join(",", v),
                    v =>
                        v.Split(",", StringSplitOptions.RemoveEmptyEntries)
                            .Select(Guid.Parse)
                            .ToList()
                )
                .Metadata.SetValueComparer(
                    new ValueComparer<IReadOnlyCollection<Guid>>(
                        (c1, c2) =>
                            (c1 == null && c2 == null)
                            || (c1 != null && c2 != null && c1.SequenceEqual(c2)),
                        c =>
                            c == null
                                ? 0
                                : c.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())),
                        c => c == null ? new List<Guid>() : c.ToList()
                    )
                );
        }
    }
}

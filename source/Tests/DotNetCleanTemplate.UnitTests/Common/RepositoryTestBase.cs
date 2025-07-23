using DotNetCleanTemplate.Domain.Entities;
using DotNetCleanTemplate.Domain.ValueObjects.Role;
using DotNetCleanTemplate.Domain.ValueObjects.User;
using DotNetCleanTemplate.Infrastructure.Persistent;
using DotNetCleanTemplate.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;

namespace DotNetCleanTemplate.UnitTests.Common
{
    public abstract class RepositoryTestBase<TContext> : TestBase
        where TContext : DbContext
    {
        protected static TContext CreateDbContext(
            Func<DbContextOptions<TContext>, TContext> factory
        )
        {
            var options = new DbContextOptionsBuilder<TContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .ConfigureWarnings(warnings =>
                    warnings.Ignore(
                        Microsoft
                            .EntityFrameworkCore
                            .Diagnostics
                            .InMemoryEventId
                            .TransactionIgnoredWarning
                    )
                )
                .Options;
            return factory(options);
        }

        protected static User CreateTestUser(string? email = null)
        {
            var passwordHasher = new PasswordHasher();
            return new User(
                new UserName("TestUser"),
                new Email(email ?? $"test{Guid.NewGuid()}@example.com"),
                new PasswordHash(passwordHasher.HashPassword("12345678901234567890"))
            );
        }

        protected static Role CreateTestRole(string? name = null)
        {
            return new Role(new RoleName(name ?? $"Role{Guid.NewGuid()}"));
        }

        protected static async Task<User> CreateAndSaveUserAsync(
            AppDbContext context,
            string? email = null
        )
        {
            var user = CreateTestUser(email);
            context.Users.Add(user);
            await context.SaveChangesAsync();
            return user;
        }

        protected static async Task<Role> CreateAndSaveRoleAsync(
            AppDbContext context,
            string? name = null
        )
        {
            var role = CreateTestRole(name);
            context.Roles.Add(role);
            await context.SaveChangesAsync();
            return role;
        }

        protected static async Task<UserRole> CreateAndSaveUserRoleAsync(
            AppDbContext context,
            User user,
            Role role
        )
        {
            var userRole = new UserRole(user, role);
            context.Set<UserRole>().Add(userRole);
            await context.SaveChangesAsync();
            return userRole;
        }
    }
}

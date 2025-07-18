using DotNetCleanTemplate.Application.Services;
using DotNetCleanTemplate.Domain.Entities;
using DotNetCleanTemplate.Domain.ValueObjects.Role;
using DotNetCleanTemplate.Domain.ValueObjects.User;
using DotNetCleanTemplate.Infrastructure.Persistent;
using DotNetCleanTemplate.Infrastructure.Persistent.Repositories;
using DotNetCleanTemplate.Infrastructure.Services;

namespace DotNetCleanTemplate.UnitTests.Common
{
    public abstract class ServiceTestBase : TestBase
    {
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

        protected static UserService CreateUserService(AppDbContext context)
        {
            var userRepository = new UserRepository(context);
            var unitOfWork = new UnitOfWork(context);
            return new UserService(userRepository, unitOfWork);
        }

        protected static RoleService CreateRoleService(AppDbContext context)
        {
            var roleRepository = new RoleRepository(context);
            var unitOfWork = new UnitOfWork(context);
            return new RoleService(roleRepository, unitOfWork);
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
    }
}

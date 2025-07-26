using DotNetCleanTemplate.Application.Configurations;
using DotNetCleanTemplate.Application.Services;
using DotNetCleanTemplate.Domain.Entities;
using DotNetCleanTemplate.Domain.Factories.Entities;
using DotNetCleanTemplate.Domain.Factories.Role;
using DotNetCleanTemplate.Domain.Factories.User;
using DotNetCleanTemplate.Domain.Services;
using DotNetCleanTemplate.Infrastructure.Factories.Entities;
using DotNetCleanTemplate.Infrastructure.Factories.Role;
using DotNetCleanTemplate.Infrastructure.Factories.User;
using DotNetCleanTemplate.Infrastructure.Persistent;
using DotNetCleanTemplate.Infrastructure.Persistent.Repositories;
using DotNetCleanTemplate.Infrastructure.Services;
using Microsoft.Extensions.Options;

namespace DotNetCleanTemplate.UnitTests.Common
{
    public abstract class ServiceTestBase : TestBase
    {
        protected static User CreateTestUser(string? email = null)
        {
            var passwordHasher = new PasswordHasher();
            var emailFactory = new EmailFactory();
            var userNameFactory = new UserNameFactory();
            var passwordHashFactory = new PasswordHashFactory();
            var userFactory = new UserFactory(emailFactory, userNameFactory, passwordHashFactory);

            return userFactory.Create(
                "TestUser",
                email ?? $"test{Guid.NewGuid()}@example.com",
                passwordHasher.HashPassword("12345678901234567890")
            );
        }

        protected static Role CreateTestRole(string? name = null)
        {
            var roleNameFactory = new RoleNameFactory();
            var roleFactory = new RoleFactory(roleNameFactory);
            return roleFactory.Create(name ?? $"Role{Guid.NewGuid()}");
        }

        protected static UserService CreateUserService(AppDbContext context)
        {
            var userRepository = new UserRepository(context);
            var roleRepository = new RoleRepository(context);
            var unitOfWork = new UnitOfWork(context);
            var passwordHasher = new PasswordHasher();
            var passwordHashFactory = new PasswordHashFactory();
            var defaultSettings = Options.Create(new DefaultSettings());
            return new UserService(
                userRepository,
                roleRepository,
                unitOfWork,
                passwordHasher,
                passwordHashFactory,
                defaultSettings
            );
        }

        protected static RoleService CreateRoleService(AppDbContext context)
        {
            var roleRepository = new RoleRepository(context);
            var unitOfWork = new UnitOfWork(context);
            var defaultSettings = Options.Create(new DefaultSettings());
            var defaultRoleService = new DefaultRoleService(defaultSettings);
            return new RoleService(roleRepository, unitOfWork, defaultSettings, defaultRoleService);
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

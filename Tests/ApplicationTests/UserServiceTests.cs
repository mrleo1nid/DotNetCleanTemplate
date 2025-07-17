using DotNetCleanTemplate.Application.Services;
using DotNetCleanTemplate.Domain.Entities;
using DotNetCleanTemplate.Domain.ValueObjects.User;
using DotNetCleanTemplate.Infrastructure.Persistent.Repositories;

namespace ApplicationTests
{
    public class UserServiceTests : TestBase
    {
        private static User CreateTestUser(string? email = null)
        {
            return new User(
                new UserName("TestUser"),
                new Email(email ?? $"test{Guid.NewGuid()}@example.com"),
                new PasswordHash("12345678901234567890")
            );
        }

        [Fact]
        public async Task CreateUserAsync_Works()
        {
            using var context = CreateDbContext();
            var userRepository = new UserRepository(context);
            var unitOfWork = new UnitOfWork(context);
            var service = new UserService(userRepository, unitOfWork);
            var user = CreateTestUser();
            var result = await service.CreateUserAsync(user);
            Assert.True(result.IsSuccess);
            Assert.Equal(user.Email.Value, result.Value.Email.Value);
        }

        [Fact]
        public async Task FindByEmailAsync_ReturnsUser()
        {
            using var context = CreateDbContext();
            var userRepository = new UserRepository(context);
            var unitOfWork = new UnitOfWork(context);
            var service = new UserService(userRepository, unitOfWork);
            var user = CreateTestUser();
            await service.CreateUserAsync(user);
            var result = await service.FindByEmailAsync(user.Email.Value);
            Assert.True(result.IsSuccess);
            Assert.Equal(user.Email.Value, result.Value.Email.Value);
        }

        [Fact]
        public async Task FindByEmailAsync_ReturnsFailureForUnknownEmail()
        {
            using var context = CreateDbContext();
            var userRepository = new UserRepository(context);
            var unitOfWork = new UnitOfWork(context);
            var service = new UserService(userRepository, unitOfWork);
            var result = await service.FindByEmailAsync("unknown@example.com");
            Assert.False(result.IsSuccess);
            Assert.Contains(result.Errors, e => e.Code == "User.NotFound");
        }

        [Fact]
        public async Task GetAllUsersWithRolesAsync_ReturnsFailure_WhenRepositoryReturnsNull()
        {
            using var context = CreateDbContext();
            var userRepository = new UserRepository(context);
            var unitOfWork = new UnitOfWork(context);
            var service = new UserService(userRepository, unitOfWork);
            // Не добавляем пользователей, эмулируем null через мок или напрямую
            // Для InMemory EF Core context.Users возвращает пустой список, а не null,
            // поэтому эмулируем через подмену метода, если потребуется.
            // Здесь тестируем на пустой список, т.к. null маловероятен для EF.
            var result = await service.GetAllUsersWithRolesAsync();
            Assert.False(result.IsSuccess);
            Assert.Contains(result.Errors, e => e.Code == "User.NotFound");
        }

        [Fact]
        public async Task AssignRoleToUserAsync_AssignsRoleSuccessfully()
        {
            using var context = CreateDbContext();
            var userRepository = new UserRepository(context);
            var unitOfWork = new UnitOfWork(context);
            var userService = new UserService(userRepository, unitOfWork);
            var user = CreateTestUser();
            var role = new DotNetCleanTemplate.Domain.Entities.Role(new("Admin"));
            await userService.CreateUserAsync(user);
            var roleRepo =
                new DotNetCleanTemplate.Infrastructure.Persistent.Repositories.RoleRepository(
                    context
                );
            var roleService = new DotNetCleanTemplate.Application.Services.RoleService(
                roleRepo,
                unitOfWork
            );
            await roleService.CreateRoleAsync(role);
            var result = await userService.AssignRoleToUserAsync(user.Id, role.Id);
            Assert.True(result.IsSuccess);
        }

        [Fact]
        public async Task AssignRoleToUserAsync_ReturnsFailure_WhenUserNotFound()
        {
            using var context = CreateDbContext();
            var userRepository = new UserRepository(context);
            var unitOfWork = new UnitOfWork(context);
            var userService = new UserService(userRepository, unitOfWork);
            var role = new DotNetCleanTemplate.Domain.Entities.Role(new("Admin"));
            var roleRepo =
                new DotNetCleanTemplate.Infrastructure.Persistent.Repositories.RoleRepository(
                    context
                );
            var roleService = new DotNetCleanTemplate.Application.Services.RoleService(
                roleRepo,
                unitOfWork
            );
            await roleService.CreateRoleAsync(role);
            var result = await userService.AssignRoleToUserAsync(Guid.NewGuid(), role.Id);
            Assert.False(result.IsSuccess);
            Assert.Contains(result.Errors, e => e.Code == "User.NotFound");
        }

        [Fact]
        public async Task AssignRoleToUserAsync_ReturnsFailure_WhenRoleNotFound()
        {
            using var context = CreateDbContext();
            var userRepository = new UserRepository(context);
            var unitOfWork = new UnitOfWork(context);
            var userService = new UserService(userRepository, unitOfWork);
            var user = CreateTestUser();
            await userService.CreateUserAsync(user);
            var result = await userService.AssignRoleToUserAsync(user.Id, Guid.NewGuid());
            Assert.False(result.IsSuccess);
            Assert.Contains(result.Errors, e => e.Code == "Role.NotFound");
        }
    }
}

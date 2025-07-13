using DotNetCleanTemplate.Application.Services;
using DotNetCleanTemplate.Domain.Entities;
using DotNetCleanTemplate.Domain.Repositories;
using DotNetCleanTemplate.Domain.ValueObjects.User;
using DotNetCleanTemplate.Infrastructure.Persistent;
using DotNetCleanTemplate.Infrastructure.Persistent.Repositories;
using Microsoft.EntityFrameworkCore;

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
            var created = await service.CreateUserAsync(user);
            Assert.NotNull(created);
            Assert.Equal(user.Email.Value, created.Email.Value);
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
            var found = await service.FindByEmailAsync(user.Email.Value);
            Assert.NotNull(found);
            Assert.Equal(user.Email.Value, found!.Email.Value);
        }

        [Fact]
        public async Task FindByEmailAsync_ReturnsNullForUnknownEmail()
        {
            using var context = CreateDbContext();
            var userRepository = new UserRepository(context);
            var unitOfWork = new UnitOfWork(context);
            var service = new UserService(userRepository, unitOfWork);
            var found = await service.FindByEmailAsync("unknown@example.com");
            Assert.Null(found);
        }
    }
}

using DotNetCleanTemplate.Application.Configurations;
using DotNetCleanTemplate.Application.Services;
using DotNetCleanTemplate.Domain.Entities;
using DotNetCleanTemplate.Domain.Repositories;
using DotNetCleanTemplate.Domain.ValueObjects.Role;
using DotNetCleanTemplate.Infrastructure.Persistent.Repositories;
using DotNetCleanTemplate.Infrastructure.Services;
using DotNetCleanTemplate.UnitTests.Common;
using Microsoft.Extensions.Options;
using Moq;

namespace DotNetCleanTemplate.UnitTests.Application
{
    public class UserServiceTests : ServiceTestBase
    {
        private static UserService CreateMockUserService(
            Mock<IUserRepository> mockUserRepository,
            Mock<IUnitOfWork> mockUnitOfWork,
            Mock<DotNetCleanTemplate.Domain.Services.IPasswordHasher> mockPasswordHasher
        )
        {
            var mockDefaultSettings = new Mock<IOptions<DefaultSettings>>();
            mockDefaultSettings.Setup(x => x.Value).Returns(new DefaultSettings());
            return new UserService(
                mockUserRepository.Object,
                mockUnitOfWork.Object,
                mockPasswordHasher.Object,
                mockDefaultSettings.Object
            );
        }

        [Fact]
        public async Task CreateUserAsync_Works()
        {
            using var context = CreateDbContext();
            var service = CreateUserService(context);
            var user = CreateTestUser();
            var result = await service.CreateUserAsync(user);
            Assert.True(result.IsSuccess);
            Assert.Equal(user.Email.Value, result.Value.Email.Value);
        }

        [Fact]
        public async Task FindByEmailAsync_ReturnsUser()
        {
            using var context = CreateDbContext();
            var service = CreateUserService(context);
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
            var service = CreateUserService(context);
            var result = await service.FindByEmailAsync("unknown@example.com");
            Assert.False(result.IsSuccess);
            Assert.Contains(result.Errors, e => e.Code == "User.NotFound");
        }

        [Fact]
        public async Task GetAllUsersWithRolesAsync_ReturnsFailure_WhenRepositoryReturnsNull()
        {
            using var context = CreateDbContext();
            var service = CreateUserService(context);
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
            var userService = CreateUserService(context);
            var user = CreateTestUser();
            var role = new DotNetCleanTemplate.Domain.Entities.Role(new("Admin"));
            await userService.CreateUserAsync(user);
            var roleService = CreateRoleService(context);
            await roleService.CreateRoleAsync(role);
            var result = await userService.AssignRoleToUserAsync(user.Id, role.Id);
            Assert.True(result.IsSuccess);
        }

        [Fact]
        public async Task AssignRoleToUserAsync_ReturnsFailure_WhenUserNotFound()
        {
            using var context = CreateDbContext();
            var userService = CreateUserService(context);
            var role = new DotNetCleanTemplate.Domain.Entities.Role(new("Admin"));
            var roleService = CreateRoleService(context);
            await roleService.CreateRoleAsync(role);
            var result = await userService.AssignRoleToUserAsync(Guid.NewGuid(), role.Id);
            Assert.False(result.IsSuccess);
            Assert.Contains(result.Errors, e => e.Code == "User.NotFound");
        }

        [Fact]
        public async Task AssignRoleToUserAsync_ReturnsFailure_WhenRoleNotFound()
        {
            using var context = CreateDbContext();
            var userService = CreateUserService(context);
            var user = CreateTestUser();
            await userService.CreateUserAsync(user);
            var result = await userService.AssignRoleToUserAsync(user.Id, Guid.NewGuid());
            Assert.False(result.IsSuccess);
            Assert.Contains(result.Errors, e => e.Code == "Role.NotFound");
        }

        [Fact]
        public async Task FindByEmailAsync_WhenRepositoryThrowsException_ShouldHandleGracefully()
        {
            // Arrange
            var mockUserRepository = new Mock<IUserRepository>();
            var mockUnitOfWork = new Mock<IUnitOfWork>();

            mockUserRepository
                .Setup(x => x.FindByEmailAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new InvalidOperationException("Database error"));

            var mockPasswordHasher =
                new Mock<DotNetCleanTemplate.Domain.Services.IPasswordHasher>();
            var mockDefaultSettings = new Mock<IOptions<DefaultSettings>>();
            mockDefaultSettings.Setup(x => x.Value).Returns(new DefaultSettings());
            var service = new UserService(
                mockUserRepository.Object,
                mockUnitOfWork.Object,
                mockPasswordHasher.Object,
                mockDefaultSettings.Object
            );

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                service.FindByEmailAsync("test@example.com")
            );
        }

        [Fact]
        public async Task CreateUserAsync_WhenRepositoryThrowsException_ShouldHandleGracefully()
        {
            // Arrange
            var mockUserRepository = new Mock<IUserRepository>();
            var mockUnitOfWork = new Mock<IUnitOfWork>();
            var user = CreateTestUser();

            mockUserRepository
                .Setup(x => x.FindByEmailAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new InvalidOperationException("Database error"));

            var mockPasswordHasher =
                new Mock<DotNetCleanTemplate.Domain.Services.IPasswordHasher>();
            var service = CreateMockUserService(
                mockUserRepository,
                mockUnitOfWork,
                mockPasswordHasher
            );

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                service.CreateUserAsync(user)
            );
        }

        [Fact]
        public async Task GetAllUsersWithRolesAsync_WhenRepositoryThrowsException_ShouldHandleGracefully()
        {
            // Arrange
            var mockUserRepository = new Mock<IUserRepository>();
            var mockUnitOfWork = new Mock<IUnitOfWork>();

            mockUserRepository
                .Setup(x => x.GetAllUsersWithRolesAsync(It.IsAny<CancellationToken>()))
                .ThrowsAsync(new InvalidOperationException("Database error"));

            var mockPasswordHasher =
                new Mock<DotNetCleanTemplate.Domain.Services.IPasswordHasher>();
            var service = CreateMockUserService(
                mockUserRepository,
                mockUnitOfWork,
                mockPasswordHasher
            );

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                service.GetAllUsersWithRolesAsync()
            );
        }

        [Fact]
        public async Task AssignRoleToUserAsync_WhenRepositoryThrowsException_ShouldHandleGracefully()
        {
            // Arrange
            var mockUserRepository = new Mock<IUserRepository>();
            var mockUnitOfWork = new Mock<IUnitOfWork>();

            mockUserRepository
                .Setup(x => x.GetByIdAsync<User>(It.IsAny<Guid>()))
                .ThrowsAsync(new InvalidOperationException("Database error"));

            var mockPasswordHasher =
                new Mock<DotNetCleanTemplate.Domain.Services.IPasswordHasher>();
            var service = CreateMockUserService(
                mockUserRepository,
                mockUnitOfWork,
                mockPasswordHasher
            );

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                service.AssignRoleToUserAsync(Guid.NewGuid(), Guid.NewGuid())
            );
        }

        [Fact]
        public async Task CreateUserAsync_WhenMappingFails_ShouldHandleGracefully()
        {
            // Arrange
            var mockUserRepository = new Mock<IUserRepository>();
            var mockUnitOfWork = new Mock<IUnitOfWork>();
            var user = CreateTestUser();

            mockUserRepository
                .Setup(x => x.FindByEmailAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((User?)null);

            mockUserRepository
                .Setup(x => x.AddAsync(It.IsAny<User>()))
                .ThrowsAsync(new InvalidOperationException("Mapping error"));

            var mockPasswordHasher =
                new Mock<DotNetCleanTemplate.Domain.Services.IPasswordHasher>();
            var service = CreateMockUserService(
                mockUserRepository,
                mockUnitOfWork,
                mockPasswordHasher
            );

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                service.CreateUserAsync(user)
            );
        }

        [Fact]
        public async Task AssignRoleToUserAsync_WhenUnitOfWorkThrowsException_ShouldHandleGracefully()
        {
            // Arrange
            var mockUserRepository = new Mock<IUserRepository>();
            var mockUnitOfWork = new Mock<IUnitOfWork>();
            var user = CreateTestUser();
            var role = new Role(new RoleName("Admin"));

            mockUserRepository
                .Setup(x => x.GetByIdAsync<User>(It.IsAny<Guid>()))
                .ReturnsAsync(user);

            mockUserRepository
                .Setup(x => x.GetByIdAsync<Role>(It.IsAny<Guid>()))
                .ReturnsAsync(role);

            mockUnitOfWork
                .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
                .ThrowsAsync(new InvalidOperationException("Save error"));

            var mockPasswordHasher =
                new Mock<DotNetCleanTemplate.Domain.Services.IPasswordHasher>();
            var service = CreateMockUserService(
                mockUserRepository,
                mockUnitOfWork,
                mockPasswordHasher
            );

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                service.AssignRoleToUserAsync(user.Id, role.Id)
            );
        }
    }
}

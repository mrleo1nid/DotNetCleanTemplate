using DotNetCleanTemplate.Application.Services;
using DotNetCleanTemplate.Domain.Entities;
using DotNetCleanTemplate.Domain.Repositories;
using DotNetCleanTemplate.Domain.ValueObjects.Role;
using DotNetCleanTemplate.Infrastructure.Persistent;
using DotNetCleanTemplate.Infrastructure.Persistent.Repositories;
using DotNetCleanTemplate.UnitTests.Common;
using Moq;

namespace DotNetCleanTemplate.UnitTests.Application
{
    public class UserServiceTests : ServiceTestBase
    {
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

        [Fact]
        public async Task FindByEmailAsync_WhenRepositoryThrowsException_ShouldHandleGracefully()
        {
            // Arrange
            var mockUserRepository = new Mock<IUserRepository>();
            var mockUnitOfWork = new Mock<IUnitOfWork>();

            mockUserRepository
                .Setup(x => x.FindByEmailAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new InvalidOperationException("Database error"));

            var service = new UserService(mockUserRepository.Object, mockUnitOfWork.Object);

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

            var service = new UserService(mockUserRepository.Object, mockUnitOfWork.Object);

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

            var service = new UserService(mockUserRepository.Object, mockUnitOfWork.Object);

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

            var service = new UserService(mockUserRepository.Object, mockUnitOfWork.Object);

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

            var service = new UserService(mockUserRepository.Object, mockUnitOfWork.Object);

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

            var service = new UserService(mockUserRepository.Object, mockUnitOfWork.Object);

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                service.AssignRoleToUserAsync(user.Id, role.Id)
            );
        }
    }
}

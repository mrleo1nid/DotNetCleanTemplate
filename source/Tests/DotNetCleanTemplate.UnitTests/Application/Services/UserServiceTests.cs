using DotNetCleanTemplate.Application.Configurations;
using DotNetCleanTemplate.Application.Services;
using DotNetCleanTemplate.Domain.Entities;
using DotNetCleanTemplate.Domain.Factories.User;
using DotNetCleanTemplate.Domain.Repositories;
using DotNetCleanTemplate.Domain.Services;
using DotNetCleanTemplate.Domain.ValueObjects.Role;
using DotNetCleanTemplate.Domain.ValueObjects.User;
using Microsoft.Extensions.Options;
using Moq;

namespace DotNetCleanTemplate.UnitTests.Application.Services;

public class UserServiceTests
{
    private const string UserNotFoundCode = "User.NotFound";
    private const string UserAlreadyExistsCode = "User.AlreadyExists";
    private const string RoleNotFoundCode = "Role.NotFound";
    private const string UserRoleAlreadyExistsCode = "UserRole.AlreadyExists";
    private const string UserRoleNotFoundCode = "UserRole.NotFound";
    private const string CannotRemoveLastAdminCode = "UserRole.CannotRemoveLastAdmin";
    private const string DatabaseErrorMessage = "Database error";
    private const string UsersNotFoundMessage = "Пользователи не найдены.";
    private const string UserAlreadyHasRoleMessage = "User already has this role.";
    private const string CannotRemoveLastAdminMessage = "Cannot remove the last admin role.";

    private readonly Mock<IUserRepository> _mockUserRepository;
    private readonly Mock<IRoleRepository> _mockRoleRepository;
    private readonly Mock<IUnitOfWork> _mockUnitOfWork;
    private readonly Mock<IPasswordHashFactory> _mockPasswordHashFactory;
    private readonly UserService _userService;

    public UserServiceTests()
    {
        _mockUserRepository = new Mock<IUserRepository>();
        _mockRoleRepository = new Mock<IRoleRepository>();
        _mockUnitOfWork = new Mock<IUnitOfWork>();
        _mockPasswordHashFactory = new Mock<IPasswordHashFactory>();
        var passwordHasher = new DotNetCleanTemplate.Infrastructure.Services.PasswordHasher();
        var mockDefaultSettings = new Mock<IOptions<DefaultSettings>>();
        mockDefaultSettings.Setup(x => x.Value).Returns(new DefaultSettings());
        _userService = new UserService(
            _mockUserRepository.Object,
            _mockRoleRepository.Object,
            _mockUnitOfWork.Object,
            passwordHasher,
            _mockPasswordHashFactory.Object,
            mockDefaultSettings.Object
        );
    }

    #region FindByEmailAsync Tests

    [Fact]
    public async Task FindByEmailAsync_WithValidEmail_ShouldReturnUser()
    {
        // Arrange
        var email = "test@example.com";
        var user = CreateTestUser(email);

        _mockUserRepository
            .Setup(x => x.FindByEmailAsync(email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        // Act
        var result = await _userService.FindByEmailAsync(email);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(user, result.Value);
        _mockUserRepository.Verify(
            x => x.FindByEmailAsync(email, It.IsAny<CancellationToken>()),
            Times.Once
        );
    }

    [Fact]
    public async Task FindByEmailAsync_WithInvalidEmail_ShouldReturnFailure()
    {
        // Arrange
        var email = "nonexistent@example.com";

        _mockUserRepository
            .Setup(x => x.FindByEmailAsync(email, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        // Act
        var result = await _userService.FindByEmailAsync(email);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(UserNotFoundCode, result.Errors[0].Code);
        Assert.Contains(email, result.Errors[0].Message);
    }

    [Fact]
    public async Task FindByEmailAsync_WhenRepositoryThrowsException_ShouldPropagateException()
    {
        // Arrange
        var email = "test@example.com";
        var expectedException = new Exception(DatabaseErrorMessage);

        _mockUserRepository
            .Setup(x => x.FindByEmailAsync(email, It.IsAny<CancellationToken>()))
            .ThrowsAsync(expectedException);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<Exception>(() =>
            _userService.FindByEmailAsync(email)
        );
        Assert.Equal(expectedException, exception);
    }

    #endregion

    #region CreateUserAsync Tests

    [Fact]
    public async Task CreateUserAsync_WithValidUser_ShouldReturnSuccess()
    {
        // Arrange
        var email = "newuser@example.com";
        var user = CreateTestUser(email);

        _mockUserRepository
            .Setup(x => x.FindByEmailAsync(email, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        _mockUserRepository.Setup(x => x.AddAsync(user)).ReturnsAsync(user);

        // Act
        var result = await _userService.CreateUserAsync(user);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(user, result.Value);
        _mockUserRepository.Verify(
            x => x.FindByEmailAsync(email, It.IsAny<CancellationToken>()),
            Times.Once
        );
        _mockUserRepository.Verify(x => x.AddAsync(user), Times.Once);
        _mockUnitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreateUserAsync_WithDuplicateEmail_ShouldReturnFailure()
    {
        // Arrange
        var email = "existing@example.com";
        var user = CreateTestUser(email);
        var existingUser = CreateTestUser(email);

        _mockUserRepository
            .Setup(x => x.FindByEmailAsync(email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingUser);

        // Act
        var result = await _userService.CreateUserAsync(user);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(UserAlreadyExistsCode, result.Errors[0].Code);
        Assert.Contains(email, result.Errors[0].Message);
        _mockUserRepository.Verify(x => x.AddAsync(It.IsAny<User>()), Times.Never);
        _mockUnitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task CreateUserAsync_WhenRepositoryThrowsException_ShouldPropagateException()
    {
        // Arrange
        var email = "test@example.com";
        var user = CreateTestUser(email);
        var expectedException = new Exception(DatabaseErrorMessage);

        _mockUserRepository
            .Setup(x => x.FindByEmailAsync(email, It.IsAny<CancellationToken>()))
            .ThrowsAsync(expectedException);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<Exception>(() =>
            _userService.CreateUserAsync(user)
        );
        Assert.Equal(expectedException, exception);
    }

    #endregion

    #region GetAllUsersWithRolesAsync Tests

    [Fact]
    public async Task GetAllUsersWithRolesAsync_WithUsers_ShouldReturnUsers()
    {
        // Arrange
        var users = new List<User>
        {
            CreateTestUser("user1@example.com"),
            CreateTestUser("user2@example.com"),
        };

        _mockUserRepository
            .Setup(x => x.GetAllUsersWithRolesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(users);

        // Act
        var result = await _userService.GetAllUsersWithRolesAsync();

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(users, result.Value);
        Assert.Equal(2, result.Value.Count);
    }

    [Fact]
    public async Task GetAllUsersWithRolesAsync_WithEmptyList_ShouldReturnFailure()
    {
        // Arrange
        var users = new List<User>();

        _mockUserRepository
            .Setup(x => x.GetAllUsersWithRolesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(users);

        // Act
        var result = await _userService.GetAllUsersWithRolesAsync();

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(UserNotFoundCode, result.Errors[0].Code);
        Assert.Equal(UsersNotFoundMessage, result.Errors[0].Message);
    }

    [Fact]
    public async Task GetAllUsersWithRolesAsync_WithNullResult_ShouldReturnFailure()
    {
        // Arrange
        _mockUserRepository
            .Setup(x => x.GetAllUsersWithRolesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync((List<User>)null!);

        // Act
        var result = await _userService.GetAllUsersWithRolesAsync();

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(UserNotFoundCode, result.Errors[0].Code);
        Assert.Equal(UsersNotFoundMessage, result.Errors[0].Message);
    }

    [Fact]
    public async Task GetAllUsersWithRolesAsync_WhenRepositoryThrowsException_ShouldPropagateException()
    {
        // Arrange
        var expectedException = new Exception(DatabaseErrorMessage);

        _mockUserRepository
            .Setup(x => x.GetAllUsersWithRolesAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(expectedException);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<Exception>(() =>
            _userService.GetAllUsersWithRolesAsync()
        );
        Assert.Equal(expectedException, exception);
    }

    #endregion

    #region AssignRoleToUserAsync Tests

    [Fact]
    public async Task AssignRoleToUserAsync_WithValidUserAndRole_ShouldReturnSuccess()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var roleId = Guid.NewGuid();
        var user = CreateTestUser("user@example.com");
        var role = CreateTestRole("Admin");

        _mockUserRepository
            .Setup(x => x.GetUserWithRolesAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _mockRoleRepository.Setup(x => x.GetByIdAsync(roleId)).ReturnsAsync(role);

        // Act
        var result = await _userService.AssignRoleToUserAsync(userId, roleId);

        // Assert
        Assert.True(result.IsSuccess);
        _mockUserRepository.Verify(
            x => x.GetUserWithRolesAsync(userId, It.IsAny<CancellationToken>()),
            Times.Once
        );
        _mockRoleRepository.Verify(x => x.GetByIdAsync(roleId), Times.Once);
        _mockUnitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task AssignRoleToUserAsync_WithNonExistentUser_ShouldReturnFailure()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var roleId = Guid.NewGuid();

        _mockUserRepository
            .Setup(x => x.GetUserWithRolesAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        // Act
        var result = await _userService.AssignRoleToUserAsync(userId, roleId);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(UserNotFoundCode, result.Errors[0].Code);
        Assert.Contains(userId.ToString(), result.Errors[0].Message);
        _mockRoleRepository.Verify(x => x.GetByIdAsync(It.IsAny<Guid>()), Times.Never);
        _mockUnitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task AssignRoleToUserAsync_WithNonExistentRole_ShouldReturnFailure()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var roleId = Guid.NewGuid();
        var user = CreateTestUser("user@example.com");

        _mockUserRepository
            .Setup(x => x.GetUserWithRolesAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _mockRoleRepository.Setup(x => x.GetByIdAsync(roleId)).ReturnsAsync((Role?)null);

        // Act
        var result = await _userService.AssignRoleToUserAsync(userId, roleId);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(RoleNotFoundCode, result.Errors[0].Code);
        Assert.Contains(roleId.ToString(), result.Errors[0].Message);
        _mockUnitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task AssignRoleToUserAsync_WithExistingUserRole_ShouldReturnFailure()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var roleId = Guid.NewGuid();
        var user = CreateTestUser("user@example.com");
        var role = CreateTestRole("Admin");

        // Устанавливаем тот же ID для роли
        role.GetType().GetProperty("Id")?.SetValue(role, roleId);

        // Добавляем существующую роль пользователю
        user.AssignRole(role);

        _mockUserRepository
            .Setup(x => x.GetUserWithRolesAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _mockRoleRepository.Setup(x => x.GetByIdAsync(roleId)).ReturnsAsync(role);

        // Act
        var result = await _userService.AssignRoleToUserAsync(userId, roleId);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(UserRoleAlreadyExistsCode, result.Errors[0].Code);
        Assert.Equal(UserAlreadyHasRoleMessage, result.Errors[0].Message);
        _mockUnitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task AssignRoleToUserAsync_WhenRepositoryThrowsException_ShouldPropagateException()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var roleId = Guid.NewGuid();
        var expectedException = new Exception(DatabaseErrorMessage);

        _mockUserRepository
            .Setup(x => x.GetUserWithRolesAsync(userId, It.IsAny<CancellationToken>()))
            .ThrowsAsync(expectedException);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<Exception>(() =>
            _userService.AssignRoleToUserAsync(userId, roleId)
        );
        Assert.Equal(expectedException, exception);
    }

    #endregion

    #region RemoveRoleFromUserAsync Tests

    [Fact]
    public async Task RemoveRoleFromUserAsync_WithValidUserAndRole_ShouldReturnSuccess()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var roleId = Guid.NewGuid();
        var user = CreateTestUser("user@example.com");
        var role = CreateTestRole("User");

        // Устанавливаем тот же ID для роли
        role.GetType().GetProperty("Id")?.SetValue(role, roleId);

        // Добавляем роль пользователю
        user.AssignRole(role);

        _mockUserRepository
            .Setup(x => x.GetUserWithRolesAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        _mockRoleRepository.Setup(x => x.GetByIdAsync(roleId)).ReturnsAsync(role);

        // Act
        var result = await _userService.RemoveRoleFromUserAsync(userId, roleId);

        // Assert
        Assert.True(result.IsSuccess);
        _mockUserRepository.Verify(
            x => x.GetUserWithRolesAsync(userId, It.IsAny<CancellationToken>()),
            Times.Once
        );
        _mockRoleRepository.Verify(x => x.GetByIdAsync(roleId), Times.Once);
        _mockUnitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task RemoveRoleFromUserAsync_WithNonExistentUser_ShouldReturnFailure()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var roleId = Guid.NewGuid();

        _mockUserRepository
            .Setup(x => x.GetUserWithRolesAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        // Act
        var result = await _userService.RemoveRoleFromUserAsync(userId, roleId);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(UserNotFoundCode, result.Errors[0].Code);
        Assert.Contains(userId.ToString(), result.Errors[0].Message);
        _mockRoleRepository.Verify(x => x.GetByIdAsync(It.IsAny<Guid>()), Times.Never);
        _mockUnitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task RemoveRoleFromUserAsync_WithNonExistentRole_ShouldReturnFailure()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var roleId = Guid.NewGuid();
        var user = CreateTestUser("user@example.com");

        _mockUserRepository
            .Setup(x => x.GetUserWithRolesAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        _mockRoleRepository.Setup(x => x.GetByIdAsync(roleId)).ReturnsAsync((Role?)null);

        // Act
        var result = await _userService.RemoveRoleFromUserAsync(userId, roleId);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(RoleNotFoundCode, result.Errors[0].Code);
        Assert.Contains(roleId.ToString(), result.Errors[0].Message);
        _mockUnitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task RemoveRoleFromUserAsync_WithUserNotHavingRole_ShouldReturnFailure()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var roleId = Guid.NewGuid();
        var user = CreateTestUser("user@example.com");
        var role = CreateTestRole("User");

        // Устанавливаем тот же ID для роли
        role.GetType().GetProperty("Id")?.SetValue(role, roleId);

        // Пользователь НЕ имеет эту роль
        _mockUserRepository
            .Setup(x => x.GetUserWithRolesAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        _mockRoleRepository.Setup(x => x.GetByIdAsync(roleId)).ReturnsAsync(role);

        // Act
        var result = await _userService.RemoveRoleFromUserAsync(userId, roleId);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(UserRoleNotFoundCode, result.Errors[0].Code);
        Assert.Equal("User does not have this role.", result.Errors[0].Message);
        _mockUnitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task RemoveRoleFromUserAsync_WithAdminRoleAndMultipleAdmins_ShouldReturnSuccess()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var roleId = Guid.NewGuid();
        var user = CreateTestUser("admin@example.com");
        var role = CreateTestRole("Admin");

        // Устанавливаем тот же ID для роли
        role.GetType().GetProperty("Id")?.SetValue(role, roleId);

        // Добавляем роль админа пользователю
        user.AssignRole(role);

        // Создаем второго пользователя с ролью админа
        var secondUser = CreateTestUser("admin2@example.com");
        secondUser.AssignRole(role);

        _mockUserRepository
            .Setup(x => x.GetUserWithRolesAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        _mockRoleRepository.Setup(x => x.GetByIdAsync(roleId)).ReturnsAsync(role);
        _mockUserRepository
            .Setup(x => x.GetUsersByRoleAsync(roleId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<User> { user, secondUser });

        // Act
        var result = await _userService.RemoveRoleFromUserAsync(userId, roleId);

        // Assert
        Assert.True(result.IsSuccess);
        _mockUserRepository.Verify(
            x => x.GetUsersByRoleAsync(roleId, It.IsAny<CancellationToken>()),
            Times.Once
        );
        _mockUnitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task RemoveRoleFromUserAsync_WithAdminRoleAndLastAdmin_ShouldReturnFailure()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var roleId = Guid.NewGuid();
        var user = CreateTestUser("admin@example.com");
        var role = CreateTestRole("Admin");

        // Устанавливаем тот же ID для роли
        role.GetType().GetProperty("Id")?.SetValue(role, roleId);

        // Добавляем роль админа пользователю
        user.AssignRole(role);

        _mockUserRepository
            .Setup(x => x.GetUserWithRolesAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        _mockRoleRepository.Setup(x => x.GetByIdAsync(roleId)).ReturnsAsync(role);
        _mockUserRepository
            .Setup(x => x.GetUsersByRoleAsync(roleId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<User> { user });

        // Act
        var result = await _userService.RemoveRoleFromUserAsync(userId, roleId);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(CannotRemoveLastAdminCode, result.Errors[0].Code);
        Assert.Equal(CannotRemoveLastAdminMessage, result.Errors[0].Message);
        _mockUserRepository.Verify(
            x => x.GetUsersByRoleAsync(roleId, It.IsAny<CancellationToken>()),
            Times.Once
        );
        _mockUnitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task RemoveRoleFromUserAsync_WithNonAdminRole_ShouldNotCheckForLastAdmin()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var roleId = Guid.NewGuid();
        var user = CreateTestUser("user@example.com");
        var role = CreateTestRole("User");

        // Устанавливаем тот же ID для роли
        role.GetType().GetProperty("Id")?.SetValue(role, roleId);

        // Добавляем роль пользователю
        user.AssignRole(role);

        _mockUserRepository
            .Setup(x => x.GetUserWithRolesAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        _mockRoleRepository.Setup(x => x.GetByIdAsync(roleId)).ReturnsAsync(role);

        // Act
        var result = await _userService.RemoveRoleFromUserAsync(userId, roleId);

        // Assert
        Assert.True(result.IsSuccess);
        // Проверяем, что метод GetUsersByRoleAsync не вызывался для не-админ ролей
        _mockUserRepository.Verify(
            x => x.GetUsersByRoleAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()),
            Times.Never
        );
        _mockUnitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    #endregion

    #region ChangeUserPasswordAsync Tests

    [Fact]
    public async Task ChangeUserPasswordAsync_WithValidUserAndPassword_ShouldReturnSuccess()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var newPassword = "newPassword123";
        var user = CreateTestUser("test@example.com");
        var passwordHash = new PasswordHash("hashed_password");

        _mockUserRepository.Setup(x => x.GetByIdAsync(userId)).ReturnsAsync(user);

        _mockPasswordHashFactory.Setup(x => x.Create(It.IsAny<string>())).Returns(passwordHash);

        // Act
        var result = await _userService.ChangeUserPasswordAsync(userId, newPassword);

        // Assert
        Assert.True(result.IsSuccess);
        _mockUserRepository.Verify(x => x.GetByIdAsync(userId), Times.Once);
        _mockPasswordHashFactory.Verify(x => x.Create(It.IsAny<string>()), Times.Once);
        _mockUnitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ChangeUserPasswordAsync_WithNonExistentUser_ShouldReturnFailure()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var newPassword = "newPassword123";

        _mockUserRepository.Setup(x => x.GetByIdAsync(userId)).ReturnsAsync((User?)null);

        // Act
        var result = await _userService.ChangeUserPasswordAsync(userId, newPassword);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(UserNotFoundCode, result.Errors[0].Code);
        Assert.Contains(userId.ToString(), result.Errors[0].Message);
        _mockUserRepository.Verify(x => x.GetByIdAsync(userId), Times.Once);
        _mockPasswordHashFactory.Verify(x => x.Create(It.IsAny<string>()), Times.Never);
        _mockUnitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    #endregion

    #region Helper Methods

    private static User CreateTestUser(string email)
    {
        return new User(
            new UserName("TestUser"),
            new Email(email),
            new PasswordHash("hashedpassword")
        );
    }

    private static Role CreateTestRole(string name)
    {
        return new Role(new RoleName(name));
    }

    #endregion
}

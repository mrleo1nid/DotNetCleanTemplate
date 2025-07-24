using DotNetCleanTemplate.Application.Services;
using DotNetCleanTemplate.Domain.Entities;
using DotNetCleanTemplate.Domain.Repositories;
using DotNetCleanTemplate.Domain.ValueObjects.Role;
using DotNetCleanTemplate.Domain.ValueObjects.User;
using DotNetCleanTemplate.Shared.Common;
using Moq;
using Xunit;

namespace DotNetCleanTemplate.UnitTests.Application.Services;

public class UserServiceTests
{
    private const string UserNotFoundCode = "User.NotFound";
    private const string UserAlreadyExistsCode = "User.AlreadyExists";
    private const string RoleNotFoundCode = "Role.NotFound";
    private const string UserRoleAlreadyExistsCode = "UserRole.AlreadyExists";
    private const string DatabaseErrorMessage = "Database error";
    private const string UsersNotFoundMessage = "Пользователи не найдены.";
    private const string UserAlreadyHasRoleMessage = "User already has this role.";

    private readonly Mock<IUserRepository> _mockUserRepository;
    private readonly Mock<IUnitOfWork> _mockUnitOfWork;
    private readonly UserService _userService;

    public UserServiceTests()
    {
        _mockUserRepository = new Mock<IUserRepository>();
        _mockUnitOfWork = new Mock<IUnitOfWork>();
        _userService = new UserService(_mockUserRepository.Object, _mockUnitOfWork.Object);
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

        _mockUserRepository.Setup(x => x.GetByIdAsync<User>(userId)).ReturnsAsync(user);

        _mockUserRepository.Setup(x => x.GetByIdAsync<Role>(roleId)).ReturnsAsync(role);

        // Act
        var result = await _userService.AssignRoleToUserAsync(userId, roleId);

        // Assert
        Assert.True(result.IsSuccess);
        _mockUserRepository.Verify(x => x.GetByIdAsync<User>(userId), Times.Once);
        _mockUserRepository.Verify(x => x.GetByIdAsync<Role>(roleId), Times.Once);
        _mockUnitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task AssignRoleToUserAsync_WithNonExistentUser_ShouldReturnFailure()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var roleId = Guid.NewGuid();

        _mockUserRepository.Setup(x => x.GetByIdAsync<User>(userId)).ReturnsAsync((User?)null);

        // Act
        var result = await _userService.AssignRoleToUserAsync(userId, roleId);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(UserNotFoundCode, result.Errors[0].Code);
        Assert.Contains(userId.ToString(), result.Errors[0].Message);
        _mockUserRepository.Verify(x => x.GetByIdAsync<Role>(It.IsAny<Guid>()), Times.Never);
        _mockUnitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task AssignRoleToUserAsync_WithNonExistentRole_ShouldReturnFailure()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var roleId = Guid.NewGuid();
        var user = CreateTestUser("user@example.com");

        _mockUserRepository.Setup(x => x.GetByIdAsync<User>(userId)).ReturnsAsync(user);

        _mockUserRepository.Setup(x => x.GetByIdAsync<Role>(roleId)).ReturnsAsync((Role?)null);

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

        _mockUserRepository.Setup(x => x.GetByIdAsync<User>(userId)).ReturnsAsync(user);

        _mockUserRepository.Setup(x => x.GetByIdAsync<Role>(roleId)).ReturnsAsync(role);

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

        _mockUserRepository.Setup(x => x.GetByIdAsync<User>(userId)).ThrowsAsync(expectedException);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<Exception>(() =>
            _userService.AssignRoleToUserAsync(userId, roleId)
        );
        Assert.Equal(expectedException, exception);
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

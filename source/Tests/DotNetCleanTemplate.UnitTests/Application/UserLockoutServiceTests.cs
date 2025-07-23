using DotNetCleanTemplate.Application.Configurations;
using DotNetCleanTemplate.Application.Services;
using DotNetCleanTemplate.Domain.Entities;
using DotNetCleanTemplate.Domain.Repositories;
using DotNetCleanTemplate.UnitTests.Common;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;

namespace DotNetCleanTemplate.UnitTests.Application;

public class UserLockoutServiceTests : ServiceTestBase
{
    private readonly Mock<IUserLockoutRepository> _mockUserLockoutRepository;
    private readonly Mock<IUnitOfWork> _mockUnitOfWork;
    private readonly FailToBanSettings _settings;
    private readonly UserLockoutService _service;

    public UserLockoutServiceTests()
    {
        _mockUserLockoutRepository = new Mock<IUserLockoutRepository>();
        _mockUnitOfWork = new Mock<IUnitOfWork>();
        _settings = new FailToBanSettings
        {
            EnableFailToBan = true,
            MaxFailedAttempts = 5,
            LockoutDurationMinutes = 15,
            ResetFailedAttemptsAfterMinutes = 30,
        };

        var mockOptions = new Mock<IOptions<FailToBanSettings>>();
        mockOptions.Setup(x => x.Value).Returns(_settings);

        var mockLogger = new Mock<ILogger<UserLockoutService>>();

        _service = new UserLockoutService(
            _mockUserLockoutRepository.Object,
            _mockUnitOfWork.Object,
            mockOptions.Object,
            mockLogger.Object
        );
    }

    [Fact]
    public async Task CheckUserLockoutAsync_WhenUserIsNotLocked_ShouldReturnSuccess()
    {
        // Arrange
        var userId = Guid.NewGuid();
        _mockUserLockoutRepository
            .Setup(x => x.IsUserLockedAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        var result = await _service.CheckUserLockoutAsync(userId);

        // Assert
        Assert.True(result.IsSuccess);
        _mockUserLockoutRepository.Verify(
            x => x.IsUserLockedAsync(userId, It.IsAny<CancellationToken>()),
            Times.Once
        );
    }

    [Fact]
    public async Task CheckUserLockoutAsync_WhenUserIsLocked_ShouldReturnFailure()
    {
        // Arrange
        var userId = Guid.NewGuid();
        _mockUserLockoutRepository
            .Setup(x => x.IsUserLockedAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _service.CheckUserLockoutAsync(userId);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Contains(result.Errors, e => e.Code == "Auth.UserLocked");
        _mockUserLockoutRepository.Verify(
            x => x.IsUserLockedAsync(userId, It.IsAny<CancellationToken>()),
            Times.Once
        );
    }

    [Fact]
    public async Task RecordFailedLoginAttemptAsync_WhenNoExistingLockout_ShouldCreateNewLockout()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var newLockout = new UserLockout(userId, DateTime.UtcNow.AddMinutes(15), 1);
        _mockUserLockoutRepository
            .Setup(x =>
                x.IncrementFailedAttemptsAsync(
                    userId,
                    _settings.MaxFailedAttempts,
                    _settings.LockoutDurationMinutes,
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync(newLockout);

        // Act
        var result = await _service.RecordFailedLoginAttemptAsync(userId);

        // Assert
        Assert.True(result.IsSuccess);
        _mockUserLockoutRepository.Verify(
            x =>
                x.IncrementFailedAttemptsAsync(
                    userId,
                    _settings.MaxFailedAttempts,
                    _settings.LockoutDurationMinutes,
                    It.IsAny<CancellationToken>()
                ),
            Times.Once
        );
        _mockUnitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task RecordFailedLoginAttemptAsync_WhenExistingLockout_ShouldIncrementFailedAttempts()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var updatedLockout = new UserLockout(userId, DateTime.UtcNow.AddMinutes(15), 4);
        _mockUserLockoutRepository
            .Setup(x =>
                x.IncrementFailedAttemptsAsync(
                    userId,
                    _settings.MaxFailedAttempts,
                    _settings.LockoutDurationMinutes,
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync(updatedLockout);

        // Act
        var result = await _service.RecordFailedLoginAttemptAsync(userId);

        // Assert
        Assert.True(result.IsSuccess);
        _mockUserLockoutRepository.Verify(
            x =>
                x.IncrementFailedAttemptsAsync(
                    userId,
                    _settings.MaxFailedAttempts,
                    _settings.LockoutDurationMinutes,
                    It.IsAny<CancellationToken>()
                ),
            Times.Once
        );
        _mockUnitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task RecordFailedLoginAttemptAsync_WhenMaxAttemptsReached_ShouldExtendLockout()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var extendedLockout = new UserLockout(userId, DateTime.UtcNow.AddMinutes(30), 6);
        _mockUserLockoutRepository
            .Setup(x =>
                x.IncrementFailedAttemptsAsync(
                    userId,
                    _settings.MaxFailedAttempts,
                    _settings.LockoutDurationMinutes,
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync(extendedLockout);

        // Act
        var result = await _service.RecordFailedLoginAttemptAsync(userId);

        // Assert
        Assert.True(result.IsSuccess);
        _mockUserLockoutRepository.Verify(
            x =>
                x.IncrementFailedAttemptsAsync(
                    userId,
                    _settings.MaxFailedAttempts,
                    _settings.LockoutDurationMinutes,
                    It.IsAny<CancellationToken>()
                ),
            Times.Once
        );
        _mockUnitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task RecordSuccessfulLoginAsync_WhenLockoutExists_ShouldClearLockout()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var existingLockout = new UserLockout(userId, DateTime.UtcNow.AddMinutes(10), 3);

        _mockUserLockoutRepository
            .Setup(x => x.GetByUserIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingLockout);

        // Act
        var result = await _service.RecordSuccessfulLoginAsync(userId);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(0, existingLockout.FailedAttempts);
        Assert.True(existingLockout.LockoutEnd < DateTime.UtcNow);
        _mockUnitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task RecordSuccessfulLoginAsync_WhenNoLockoutExists_ShouldReturnSuccess()
    {
        // Arrange
        var userId = Guid.NewGuid();
        _mockUserLockoutRepository
            .Setup(x => x.GetByUserIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((UserLockout?)null);

        // Act
        var result = await _service.RecordSuccessfulLoginAsync(userId);

        // Assert
        Assert.True(result.IsSuccess);
        _mockUserLockoutRepository.Verify(
            x => x.GetByUserIdAsync(userId, It.IsAny<CancellationToken>()),
            Times.Once
        );
        _mockUnitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ClearUserLockoutAsync_WhenLockoutExists_ShouldClearLockout()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var existingLockout = new UserLockout(userId, DateTime.UtcNow.AddMinutes(10), 3);

        _mockUserLockoutRepository
            .Setup(x => x.GetByUserIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingLockout);

        // Act
        var result = await _service.ClearUserLockoutAsync(userId);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(0, existingLockout.FailedAttempts);
        Assert.True(existingLockout.LockoutEnd < DateTime.UtcNow);
        _mockUnitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CheckUserLockoutAsync_WhenFailToBanDisabled_ShouldReturnSuccess()
    {
        // Arrange
        var userId = Guid.NewGuid();
        _settings.EnableFailToBan = false;

        // Act
        var result = await _service.CheckUserLockoutAsync(userId);

        // Assert
        Assert.True(result.IsSuccess);
        _mockUserLockoutRepository.Verify(
            x => x.IsUserLockedAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()),
            Times.Never
        );
    }

    [Fact]
    public async Task RecordFailedLoginAttemptAsync_WhenFailToBanDisabled_ShouldReturnSuccess()
    {
        // Arrange
        var userId = Guid.NewGuid();
        _settings.EnableFailToBan = false;

        // Act
        var result = await _service.RecordFailedLoginAttemptAsync(userId);

        // Assert
        Assert.True(result.IsSuccess);
        _mockUserLockoutRepository.Verify(
            x =>
                x.IncrementFailedAttemptsAsync(
                    It.IsAny<Guid>(),
                    It.IsAny<int>(),
                    It.IsAny<int>(),
                    It.IsAny<CancellationToken>()
                ),
            Times.Never
        );
    }

    [Fact]
    public async Task RecordSuccessfulLoginAsync_WhenFailToBanDisabled_ShouldReturnSuccess()
    {
        // Arrange
        var userId = Guid.NewGuid();
        _settings.EnableFailToBan = false;

        // Act
        var result = await _service.RecordSuccessfulLoginAsync(userId);

        // Assert
        Assert.True(result.IsSuccess);
        _mockUserLockoutRepository.Verify(
            x => x.GetByUserIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()),
            Times.Never
        );
        _mockUnitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ClearUserLockoutAsync_WhenUserHasNoLockout_ShouldReturnSuccess()
    {
        // Arrange
        var userId = Guid.NewGuid();
        _mockUserLockoutRepository
            .Setup(x => x.GetByUserIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((UserLockout?)null);

        // Act
        var result = await _service.ClearUserLockoutAsync(userId);

        // Assert
        Assert.True(result.IsSuccess);
        _mockUserLockoutRepository.Verify(
            x => x.GetByUserIdAsync(userId, It.IsAny<CancellationToken>()),
            Times.Once
        );
        _mockUnitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task CheckUserLockoutAsync_WhenRepositoryThrowsException_ShouldHandleGracefully()
    {
        // Arrange
        var userId = Guid.NewGuid();
        _mockUserLockoutRepository
            .Setup(x => x.IsUserLockedAsync(userId, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Database error"));

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _service.CheckUserLockoutAsync(userId)
        );
    }

    [Fact]
    public async Task RecordFailedLoginAttemptAsync_WhenRepositoryThrowsException_ShouldHandleGracefully()
    {
        // Arrange
        var userId = Guid.NewGuid();
        _mockUserLockoutRepository
            .Setup(x =>
                x.IncrementFailedAttemptsAsync(
                    userId,
                    _settings.MaxFailedAttempts,
                    _settings.LockoutDurationMinutes,
                    It.IsAny<CancellationToken>()
                )
            )
            .ThrowsAsync(new InvalidOperationException("Database error"));

        // Act
        var result = await _service.RecordFailedLoginAttemptAsync(userId);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal("Lockout.Error", result.Errors[0].Code);
        _mockUnitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task RecordSuccessfulLoginAsync_WhenRepositoryThrowsException_ShouldHandleGracefully()
    {
        // Arrange
        var userId = Guid.NewGuid();
        _mockUserLockoutRepository
            .Setup(x => x.GetByUserIdAsync(userId, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Database error"));

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _service.RecordSuccessfulLoginAsync(userId)
        );
    }

    [Fact]
    public async Task ClearUserLockoutAsync_WhenRepositoryThrowsException_ShouldHandleGracefully()
    {
        // Arrange
        var userId = Guid.NewGuid();
        _mockUserLockoutRepository
            .Setup(x => x.GetByUserIdAsync(userId, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Database error"));

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _service.ClearUserLockoutAsync(userId)
        );
    }

    [Fact]
    public async Task RecordSuccessfulLoginAsync_WhenUnitOfWorkThrowsException_ShouldHandleGracefully()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var existingLockout = new UserLockout(userId, DateTime.UtcNow.AddMinutes(10), 3);

        _mockUserLockoutRepository
            .Setup(x => x.GetByUserIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingLockout);

        _mockUnitOfWork
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Save error"));

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _service.RecordSuccessfulLoginAsync(userId)
        );
    }

    [Fact]
    public async Task ClearUserLockoutAsync_WhenUnitOfWorkThrowsException_ShouldHandleGracefully()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var existingLockout = new UserLockout(userId, DateTime.UtcNow.AddMinutes(10), 3);

        _mockUserLockoutRepository
            .Setup(x => x.GetByUserIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingLockout);

        _mockUnitOfWork
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Save error"));

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _service.ClearUserLockoutAsync(userId)
        );
    }

    [Fact]
    public async Task RecordFailedLoginAttemptAsync_WhenUnitOfWorkThrowsException_ShouldHandleGracefully()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var newLockout = new UserLockout(userId, DateTime.UtcNow.AddMinutes(15), 1);
        _mockUserLockoutRepository
            .Setup(x =>
                x.IncrementFailedAttemptsAsync(
                    userId,
                    _settings.MaxFailedAttempts,
                    _settings.LockoutDurationMinutes,
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync(newLockout);

        _mockUnitOfWork
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Save error"));

        // Act
        var result = await _service.RecordFailedLoginAttemptAsync(userId);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal("Lockout.Error", result.Errors[0].Code);
    }
}

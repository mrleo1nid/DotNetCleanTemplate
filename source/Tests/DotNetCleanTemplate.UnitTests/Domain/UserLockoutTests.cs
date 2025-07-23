using DotNetCleanTemplate.Domain.Entities;
using DotNetCleanTemplate.UnitTests.Common;

namespace DotNetCleanTemplate.UnitTests.Domain;

public class UserLockoutTests : TestBase
{
    [Fact]
    public void Constructor_WithValidParameters_ShouldCreateUserLockout()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var lockoutEnd = DateTime.UtcNow.AddMinutes(15);
        var failedAttempts = 3;

        // Act
        var userLockout = new UserLockout(userId, lockoutEnd, failedAttempts);

        // Assert
        Assert.NotEqual(Guid.Empty, userLockout.Id);
        Assert.Equal(userId, userLockout.UserId);
        Assert.Equal(lockoutEnd, userLockout.LockoutEnd);
        Assert.Equal(failedAttempts, userLockout.FailedAttempts);
        Assert.True(userLockout.IsLocked);
        Assert.NotEqual(default, userLockout.CreatedAt);
    }

    [Fact]
    public void Constructor_WithDefaultFailedAttempts_ShouldSetZero()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var lockoutEnd = DateTime.UtcNow.AddMinutes(15);

        // Act
        var userLockout = new UserLockout(userId, lockoutEnd);

        // Assert
        Assert.Equal(0, userLockout.FailedAttempts);
    }

    [Fact]
    public void IsLocked_WhenLockoutEndIsInFuture_ShouldReturnTrue()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var lockoutEnd = DateTime.UtcNow.AddMinutes(15);
        var userLockout = new UserLockout(userId, lockoutEnd);

        // Act
        var isLocked = userLockout.IsLocked;

        // Assert
        Assert.True(isLocked);
    }

    [Fact]
    public void IsLocked_WhenLockoutEndIsInPast_ShouldReturnFalse()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var lockoutEnd = DateTime.UtcNow.AddMinutes(-15);
        var userLockout = new UserLockout(userId, lockoutEnd);

        // Act
        var isLocked = userLockout.IsLocked;

        // Assert
        Assert.False(isLocked);
    }

    [Fact]
    public void IncrementFailedAttempts_ShouldIncrementCounter()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var lockoutEnd = DateTime.UtcNow.AddMinutes(15);
        var userLockout = new UserLockout(userId, lockoutEnd, 3);
        var originalUpdatedAt = userLockout.UpdatedAt;

        // Act
        userLockout.IncrementFailedAttempts();

        // Assert
        Assert.Equal(4, userLockout.FailedAttempts);
        Assert.True(userLockout.UpdatedAt > originalUpdatedAt);
    }

    [Fact]
    public void IncrementFailedAttempts_ShouldUpdateTimestamp()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var lockoutEnd = DateTime.UtcNow.AddMinutes(15);
        var userLockout = new UserLockout(userId, lockoutEnd, 3);
        var originalUpdatedAt = userLockout.UpdatedAt;

        // Act
        userLockout.IncrementFailedAttempts();

        // Assert
        Assert.True(userLockout.UpdatedAt >= originalUpdatedAt);
    }

    [Fact]
    public void ResetFailedAttempts_ShouldResetToZero()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var lockoutEnd = DateTime.UtcNow.AddMinutes(15);
        var userLockout = new UserLockout(userId, lockoutEnd, 5);
        var originalUpdatedAt = userLockout.UpdatedAt;

        // Act
        userLockout.ResetFailedAttempts();

        // Assert
        Assert.Equal(0, userLockout.FailedAttempts);
        Assert.True(userLockout.UpdatedAt > originalUpdatedAt);
    }

    [Fact]
    public void ResetFailedAttempts_ShouldUpdateTimestamp()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var lockoutEnd = DateTime.UtcNow.AddMinutes(15);
        var userLockout = new UserLockout(userId, lockoutEnd, 5);
        var originalUpdatedAt = userLockout.UpdatedAt;

        // Act
        userLockout.ResetFailedAttempts();

        // Assert
        Assert.True(userLockout.UpdatedAt >= originalUpdatedAt);
    }

    [Fact]
    public void ExtendLockout_ShouldUpdateLockoutEnd()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var originalLockoutEnd = DateTime.UtcNow.AddMinutes(15);
        var newLockoutEnd = DateTime.UtcNow.AddMinutes(30);
        var userLockout = new UserLockout(userId, originalLockoutEnd, 3);
        var originalUpdatedAt = userLockout.UpdatedAt;

        // Act
        userLockout.ExtendLockout(newLockoutEnd);

        // Assert
        Assert.Equal(newLockoutEnd, userLockout.LockoutEnd);
        Assert.True(userLockout.UpdatedAt >= originalUpdatedAt);
    }

    [Fact]
    public void ExtendLockout_ShouldUpdateTimestamp()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var originalLockoutEnd = DateTime.UtcNow.AddMinutes(15);
        var newLockoutEnd = DateTime.UtcNow.AddMinutes(30);
        var userLockout = new UserLockout(userId, originalLockoutEnd, 3);
        var originalUpdatedAt = userLockout.UpdatedAt;

        // Act
        userLockout.ExtendLockout(newLockoutEnd);

        // Assert
        Assert.True(userLockout.UpdatedAt >= originalUpdatedAt);
    }

    [Fact]
    public void ClearLockout_ShouldSetLockoutEndToPast()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var lockoutEnd = DateTime.UtcNow.AddMinutes(15);
        var userLockout = new UserLockout(userId, lockoutEnd, 5);
        var originalUpdatedAt = userLockout.UpdatedAt;

        // Act
        userLockout.ClearLockout();

        // Assert
        Assert.True(userLockout.LockoutEnd < DateTime.UtcNow);
        Assert.Equal(0, userLockout.FailedAttempts);
        Assert.True(userLockout.UpdatedAt >= originalUpdatedAt);
    }

    [Fact]
    public void ClearLockout_ShouldResetFailedAttempts()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var lockoutEnd = DateTime.UtcNow.AddMinutes(15);
        var userLockout = new UserLockout(userId, lockoutEnd, 5);

        // Act
        userLockout.ClearLockout();

        // Assert
        Assert.Equal(0, userLockout.FailedAttempts);
    }

    [Fact]
    public void ClearLockout_ShouldUpdateTimestamp()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var lockoutEnd = DateTime.UtcNow.AddMinutes(15);
        var userLockout = new UserLockout(userId, lockoutEnd, 5);
        var originalUpdatedAt = userLockout.UpdatedAt;

        // Act
        userLockout.ClearLockout();

        // Assert
        Assert.True(userLockout.UpdatedAt >= originalUpdatedAt);
    }

    [Fact]
    public void PrivateConstructor_ShouldBeAccessibleForEF()
    {
        // Arrange & Act
        var userLockout = (UserLockout)Activator.CreateInstance(typeof(UserLockout), true)!;

        // Assert
        Assert.NotNull(userLockout);
        Assert.Equal(Guid.Empty, userLockout.Id);
        Assert.Equal(Guid.Empty, userLockout.UserId);
        Assert.Equal(default, userLockout.LockoutEnd);
        Assert.Equal(0, userLockout.FailedAttempts);
    }
}

using DotNetCleanTemplate.Domain.Entities;
using DotNetCleanTemplate.Infrastructure.Persistent;
using DotNetCleanTemplate.Infrastructure.Persistent.Repositories;
using DotNetCleanTemplate.UnitTests.Common;
using Microsoft.EntityFrameworkCore;

namespace DotNetCleanTemplate.UnitTests.Infrastructure;

public class UserLockoutRepositoryTests : RepositoryTestBase<AppDbContext>
{
    private readonly UserLockoutRepository _userLockoutRepository;
    private readonly AppDbContext _context;

    public UserLockoutRepositoryTests()
    {
        _context = CreateDbContext(options => new AppDbContext(options));
        _userLockoutRepository = new UserLockoutRepository(_context);
    }

    [Fact]
    public async Task GetByUserIdAsync_WhenUserLockoutExists_ShouldReturnLockout()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var lockout = new UserLockout(userId, DateTime.UtcNow.AddMinutes(30));
        await _context.UserLockouts.AddAsync(lockout);
        await _context.SaveChangesAsync();

        // Act
        var result = await _userLockoutRepository.GetByUserIdAsync(userId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(userId, result.UserId);
        Assert.Equal(lockout.Id, result.Id);
    }

    [Fact]
    public async Task GetByUserIdAsync_WhenUserLockoutNotExists_ShouldReturnNull()
    {
        // Arrange
        var userId = Guid.NewGuid();

        // Act
        var result = await _userLockoutRepository.GetByUserIdAsync(userId);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task IsUserLockedAsync_WhenUserIsLocked_ShouldReturnTrue()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var lockout = new UserLockout(userId, DateTime.UtcNow.AddMinutes(30));
        await _context.UserLockouts.AddAsync(lockout);
        await _context.SaveChangesAsync();

        // Act
        var result = await _userLockoutRepository.IsUserLockedAsync(userId);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task IsUserLockedAsync_WhenUserIsNotLocked_ShouldReturnFalse()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var lockout = new UserLockout(userId, DateTime.UtcNow.AddMinutes(-30));
        await _context.UserLockouts.AddAsync(lockout);
        await _context.SaveChangesAsync();

        // Act
        var result = await _userLockoutRepository.IsUserLockedAsync(userId);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task IsUserLockedAsync_WhenUserLockoutNotExists_ShouldReturnFalse()
    {
        // Arrange
        var userId = Guid.NewGuid();

        // Act
        var result = await _userLockoutRepository.IsUserLockedAsync(userId);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task ClearExpiredLockoutsAsync_ShouldClearExpiredLockouts()
    {
        // Arrange
        var userId1 = Guid.NewGuid();
        var userId2 = Guid.NewGuid();
        var userId3 = Guid.NewGuid();

        var expiredLockout1 = new UserLockout(userId1, DateTime.UtcNow.AddMinutes(-30));
        var expiredLockout2 = new UserLockout(userId2, DateTime.UtcNow.AddMinutes(-60));
        var activeLockout = new UserLockout(userId3, DateTime.UtcNow.AddMinutes(30));

        await _context.UserLockouts.AddRangeAsync(expiredLockout1, expiredLockout2, activeLockout);
        await _context.SaveChangesAsync();

        // Act
        await _userLockoutRepository.ClearExpiredLockoutsAsync();

        // Assert
        var remainingLockouts = await _context.UserLockouts.ToListAsync();
        Assert.Equal(3, remainingLockouts.Count); // Все записи остаются, но истекшие очищаются

        var clearedLockout1 = remainingLockouts.FirstOrDefault(x => x.UserId == userId1);
        var clearedLockout2 = remainingLockouts.FirstOrDefault(x => x.UserId == userId2);
        var activeLockoutResult = remainingLockouts.FirstOrDefault(x => x.UserId == userId3);

        Assert.NotNull(clearedLockout1);
        Assert.NotNull(clearedLockout2);
        Assert.NotNull(activeLockoutResult);

        Assert.False(clearedLockout1.IsLocked);
        Assert.False(clearedLockout2.IsLocked);
        Assert.True(activeLockoutResult.IsLocked);

        Assert.Equal(0, clearedLockout1.FailedAttempts);
        Assert.Equal(0, clearedLockout2.FailedAttempts);
    }

    [Fact]
    public async Task AddAsync_ShouldAddNewLockout()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var lockout = new UserLockout(userId, DateTime.UtcNow.AddMinutes(30));

        // Act
        var result = await _userLockoutRepository.AddAsync(lockout);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(userId, result.UserId);
        Assert.NotEqual(Guid.Empty, result.Id);

        var savedLockout = await _context.UserLockouts.FindAsync(result.Id);
        Assert.NotNull(savedLockout);
        Assert.Equal(userId, savedLockout.UserId);
    }
}

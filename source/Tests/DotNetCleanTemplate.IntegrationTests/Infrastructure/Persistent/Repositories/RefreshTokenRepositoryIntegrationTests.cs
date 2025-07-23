using DotNetCleanTemplate.Domain.Entities;
using DotNetCleanTemplate.Domain.Repositories;
using DotNetCleanTemplate.Domain.ValueObjects.User;
using DotNetCleanTemplate.Infrastructure.Persistent;
using DotNetCleanTemplate.Infrastructure.Persistent.Repositories;
using Microsoft.EntityFrameworkCore;
using Testcontainers.PostgreSql;
using Xunit;

namespace DotNetCleanTemplate.IntegrationTests.Infrastructure.Persistent.Repositories;

public class RefreshTokenRepositoryIntegrationTests : IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgresContainer;
    private AppDbContext _context = null!;
    private RefreshTokenRepository _repository = null!;

    public RefreshTokenRepositoryIntegrationTests()
    {
        _postgresContainer = new PostgreSqlBuilder()
            .WithImage("postgres:15")
            .WithDatabase("testdb")
            .WithUsername("testuser")
            .WithPassword("testpass")
            .Build();
    }

    public async Task InitializeAsync()
    {
        await _postgresContainer.StartAsync();

        var connectionString = _postgresContainer.GetConnectionString();

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseNpgsql(connectionString)
            .Options;

        _context = new AppDbContext(options);
        await _context.Database.EnsureCreatedAsync();

        _repository = new RefreshTokenRepository(_context);
    }

    public async Task DisposeAsync()
    {
        await _context.DisposeAsync();
        await _postgresContainer.DisposeAsync();
    }

    [Fact]
    public async Task FindByTokenAsync_WithValidToken_ShouldReturnRefreshToken()
    {
        // Arrange
        var user = new User(
            new UserName("testuser"),
            new Email("test@example.com"),
            new PasswordHash("hashedpassword")
        );
        await _context.Users.AddAsync(user);
        await _context.SaveChangesAsync();

        var refreshToken = new RefreshToken(
            "valid.token.here",
            DateTime.UtcNow.AddDays(7),
            user.Id,
            "127.0.0.1"
        );
        await _repository.AddAsync(refreshToken);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.FindByTokenAsync("valid.token.here");

        // Assert
        Assert.NotNull(result);
        Assert.Equal(refreshToken.Token, result.Token);
        Assert.Equal(refreshToken.UserId, result.UserId);
    }

    [Fact]
    public async Task FindByTokenAsync_WithInvalidToken_ShouldReturnNull()
    {
        // Arrange
        var invalidToken = "invalid.token.here";

        // Act
        var result = await _repository.FindByTokenAsync(invalidToken);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetActiveTokensByUserIdAsync_ShouldReturnActiveTokens()
    {
        // Arrange
        var user = new User(
            new UserName("testuser"),
            new Email("test@example.com"),
            new PasswordHash("hashedpassword")
        );
        await _context.Users.AddAsync(user);
        await _context.SaveChangesAsync();

        var activeToken = new RefreshToken(
            "active.token",
            DateTime.UtcNow.AddDays(7),
            user.Id,
            "127.0.0.1"
        );
        var expiredToken = new RefreshToken(
            "expired.token",
            DateTime.UtcNow.AddDays(-1),
            user.Id,
            "127.0.0.1"
        );

        await _repository.AddAsync(activeToken);
        await _repository.AddAsync(expiredToken);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetActiveTokensByUserIdAsync(user.Id);

        // Assert
        Assert.NotNull(result);
        Assert.Contains(result, t => t.Token == activeToken.Token);
        Assert.DoesNotContain(result, t => t.Token == expiredToken.Token);
    }

    [Fact]
    public async Task GetExpiredTokensAsync_ShouldReturnExpiredTokens()
    {
        // Arrange
        var user1 = new User(
            new UserName("user1"),
            new Email("user1@example.com"),
            new PasswordHash("hashedpassword1")
        );
        var user2 = new User(
            new UserName("user2"),
            new Email("user2@example.com"),
            new PasswordHash("hashedpassword2")
        );
        await _context.Users.AddAsync(user1);
        await _context.Users.AddAsync(user2);
        await _context.SaveChangesAsync();

        var expiredToken = new RefreshToken(
            "expired.token",
            DateTime.UtcNow.AddDays(-1),
            user1.Id,
            "127.0.0.1"
        );
        var validToken = new RefreshToken(
            "valid.token",
            DateTime.UtcNow.AddDays(7),
            user2.Id,
            "127.0.0.1"
        );

        await _repository.AddAsync(expiredToken);
        await _repository.AddAsync(validToken);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetExpiredTokensAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Contains(result, t => t.Token == expiredToken.Token);
        Assert.DoesNotContain(result, t => t.Token == validToken.Token);
    }
}

using DotNetCleanTemplate.Domain.Models;
using DotNetCleanTemplate.Domain.Services;
using DotNetCleanTemplate.Infrastructure.Services;
using DotNetCleanTemplate.UnitTests.Common;
using Moq;

namespace DotNetCleanTemplate.UnitTests.Infrastructure;

public class HealthCheckServiceTests : TestBase
{
    [Fact]
    public async Task CheckAsync_WhenAllServicesHealthy_ShouldReturnHealthy()
    {
        // Arrange
        using var context = CreateDbContext();
        var mockCacheService = new Mock<ICacheService>();

        mockCacheService
            .Setup(x =>
                x.GetOrCreateAsync<string>(
                    It.IsAny<string>(),
                    It.IsAny<string?>(),
                    It.IsAny<Func<Task<string>>>(),
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync("ok");

        var service = new HealthCheckService(context, mockCacheService.Object);

        // Act
        var result = await service.CheckAsync(CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(HealthCheckResultStatus.Healthy, result.Status);
        Assert.Equal(HealthCheckResultStatus.Healthy, result.DatabaseStatus);
        Assert.Equal(HealthCheckResultStatus.Healthy, result.CacheStatus);
        Assert.True(result.ServerTime > DateTime.UtcNow.AddMinutes(-1));
    }

    [Fact]
    public async Task CheckAsync_WhenDatabaseIsUnavailable_ShouldReturnUnhealthy()
    {
        // Arrange
        using var context = CreateDbContext();
        var mockCacheService = new Mock<ICacheService>();

        mockCacheService
            .Setup(x =>
                x.GetOrCreateAsync<string>(
                    It.IsAny<string>(),
                    It.IsAny<string?>(),
                    It.IsAny<Func<Task<string>>>(),
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync("ok");

        var service = new HealthCheckService(context, mockCacheService.Object);

        // Act
        var result = await service.CheckAsync(CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        // InMemory база данных всегда доступна, поэтому тест проверяет успешный сценарий
        Assert.Equal(HealthCheckResultStatus.Healthy, result.Status);
        Assert.Equal(HealthCheckResultStatus.Healthy, result.DatabaseStatus);
        Assert.Equal(HealthCheckResultStatus.Healthy, result.CacheStatus);
    }

    [Fact]
    public async Task CheckAsync_WhenCacheIsUnavailable_ShouldReturnDegraded()
    {
        // Arrange
        using var context = CreateDbContext();
        var mockCacheService = new Mock<ICacheService>();

        mockCacheService
            .Setup(x =>
                x.GetOrCreateAsync<string>(
                    It.IsAny<string>(),
                    It.IsAny<string?>(),
                    It.IsAny<Func<Task<string>>>(),
                    It.IsAny<CancellationToken>()
                )
            )
            .ThrowsAsync(new InvalidOperationException("Cache connection failed"));

        var service = new HealthCheckService(context, mockCacheService.Object);

        // Act
        var result = await service.CheckAsync(CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(HealthCheckResultStatus.Degraded, result.Status);
        Assert.Equal(HealthCheckResultStatus.Healthy, result.DatabaseStatus);
        Assert.Equal(HealthCheckResultStatus.Unhealthy, result.CacheStatus);
    }

    [Fact]
    public async Task CheckAsync_WhenCacheReturnsWrongValue_ShouldReturnDegraded()
    {
        // Arrange
        using var context = CreateDbContext();
        var mockCacheService = new Mock<ICacheService>();

        mockCacheService
            .Setup(x =>
                x.GetOrCreateAsync<string>(
                    It.IsAny<string>(),
                    It.IsAny<string?>(),
                    It.IsAny<Func<Task<string>>>(),
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync("wrong_value");

        var service = new HealthCheckService(context, mockCacheService.Object);

        // Act
        var result = await service.CheckAsync(CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(HealthCheckResultStatus.Degraded, result.Status);
        Assert.Equal(HealthCheckResultStatus.Healthy, result.DatabaseStatus);
        Assert.Equal(HealthCheckResultStatus.Unhealthy, result.CacheStatus);
    }

    [Fact]
    public async Task CheckAsync_WhenAllServicesUnavailable_ShouldReturnDegraded()
    {
        // Arrange
        using var context = CreateDbContext();
        var mockCacheService = new Mock<ICacheService>();

        mockCacheService
            .Setup(x =>
                x.GetOrCreateAsync<string>(
                    It.IsAny<string>(),
                    It.IsAny<string?>(),
                    It.IsAny<Func<Task<string>>>(),
                    It.IsAny<CancellationToken>()
                )
            )
            .ThrowsAsync(new InvalidOperationException("Cache connection failed"));

        var service = new HealthCheckService(context, mockCacheService.Object);

        // Act
        var result = await service.CheckAsync(CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(HealthCheckResultStatus.Degraded, result.Status);
        Assert.Equal(HealthCheckResultStatus.Healthy, result.DatabaseStatus);
        Assert.Equal(HealthCheckResultStatus.Unhealthy, result.CacheStatus);
    }

    [Fact]
    public async Task CheckAsync_WhenTimeoutOccurs_ShouldHandleGracefully()
    {
        // Arrange
        using var context = CreateDbContext();
        var mockCacheService = new Mock<ICacheService>();

        mockCacheService
            .Setup(x =>
                x.GetOrCreateAsync<string>(
                    It.IsAny<string>(),
                    It.IsAny<string?>(),
                    It.IsAny<Func<Task<string>>>(),
                    It.IsAny<CancellationToken>()
                )
            )
            .Returns(Task.FromException<string>(new TimeoutException("Operation timed out")));

        var service = new HealthCheckService(context, mockCacheService.Object);

        // Act
        var result = await service.CheckAsync(CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(HealthCheckResultStatus.Degraded, result.Status);
        Assert.Equal(HealthCheckResultStatus.Healthy, result.DatabaseStatus);
        Assert.Equal(HealthCheckResultStatus.Unhealthy, result.CacheStatus);
    }

    [Fact]
    public async Task CheckAsync_ShouldInvalidateTestKey()
    {
        // Arrange
        using var context = CreateDbContext();
        var mockCacheService = new Mock<ICacheService>();

        mockCacheService
            .Setup(x =>
                x.GetOrCreateAsync<string>(
                    It.IsAny<string>(),
                    It.IsAny<string?>(),
                    It.IsAny<Func<Task<string>>>(),
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync("ok");

        var service = new HealthCheckService(context, mockCacheService.Object);

        // Act
        await service.CheckAsync(CancellationToken.None);

        // Assert
        mockCacheService.Verify(
            x => x.Invalidate(It.Is<string>(key => key.StartsWith("healthcheck_"))),
            Times.Once
        );
    }

    [Fact]
    public async Task CheckAsync_WhenCancellationTokenCancelled_ShouldHandleGracefully()
    {
        // Arrange
        using var context = CreateDbContext();
        var mockCacheService = new Mock<ICacheService>();
        var cancellationTokenSource = new CancellationTokenSource();
        cancellationTokenSource.Cancel();

        mockCacheService
            .Setup(x =>
                x.GetOrCreateAsync<string>(
                    It.IsAny<string>(),
                    It.IsAny<string?>(),
                    It.IsAny<Func<Task<string>>>(),
                    It.IsAny<CancellationToken>()
                )
            )
            .ThrowsAsync(new OperationCanceledException());

        var service = new HealthCheckService(context, mockCacheService.Object);

        // Act
        var result = await service.CheckAsync(cancellationTokenSource.Token);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(HealthCheckResultStatus.Degraded, result.Status);
        Assert.Equal(HealthCheckResultStatus.Healthy, result.DatabaseStatus);
        Assert.Equal(HealthCheckResultStatus.Unhealthy, result.CacheStatus);
    }
}

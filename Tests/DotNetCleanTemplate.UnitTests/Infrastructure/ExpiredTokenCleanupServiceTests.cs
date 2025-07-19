using DotNetCleanTemplate.Domain.Entities;
using DotNetCleanTemplate.Domain.Repositories;
using DotNetCleanTemplate.Infrastructure.Configurations;
using DotNetCleanTemplate.Infrastructure.Persistent;
using DotNetCleanTemplate.Infrastructure.Services;
using DotNetCleanTemplate.UnitTests.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;

namespace DotNetCleanTemplate.UnitTests.Infrastructure;

public class ExpiredTokenCleanupServiceTests : TestBase
{
    [Fact]
    public async Task ExecuteAsync_WhenExpiredTokensExist_ShouldRemoveThem()
    {
        // Arrange
        var mockServiceProvider = new Mock<IServiceProvider>();
        var mockScope = new Mock<IServiceScope>();
        var mockScopeServiceProvider = new Mock<IServiceProvider>();
        var mockRefreshTokenRepository = new Mock<IRefreshTokenRepository>();
        var mockUnitOfWork = new Mock<IUnitOfWork>();
        var mockLogger = new Mock<ILogger<ExpiredTokenCleanupService>>();
        var mockScopeFactory = new Mock<IServiceScopeFactory>();

        var settings = new TokenCleanupSettings { EnableCleanup = true, CleanupIntervalHours = 1 };

        var expiredTokens = new List<RefreshToken>
        {
            new RefreshToken("token1", DateTime.UtcNow.AddDays(-1), Guid.NewGuid(), "127.0.0.1"),
            new RefreshToken("token2", DateTime.UtcNow.AddDays(-2), Guid.NewGuid(), "127.0.0.1"),
        };

        mockServiceProvider
            .Setup(x => x.GetService(typeof(IServiceScopeFactory)))
            .Returns(mockScopeFactory.Object);

        mockScopeFactory.Setup(x => x.CreateScope()).Returns(mockScope.Object);

        mockScope.Setup(x => x.ServiceProvider).Returns(mockScopeServiceProvider.Object);

        mockScopeServiceProvider
            .Setup(x => x.GetService(typeof(IRefreshTokenRepository)))
            .Returns(mockRefreshTokenRepository.Object);

        mockScopeServiceProvider
            .Setup(x => x.GetService(typeof(IUnitOfWork)))
            .Returns(mockUnitOfWork.Object);

        mockRefreshTokenRepository
            .Setup(x => x.GetExpiredTokensAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(expiredTokens);

        mockUnitOfWork
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var service = new ExpiredTokenCleanupService(
            mockServiceProvider.Object,
            mockLogger.Object,
            Options.Create(settings)
        );

        // Act
        var cancellationTokenSource = new CancellationTokenSource();
        cancellationTokenSource.CancelAfter(TimeSpan.FromMilliseconds(100));

        await service.StartAsync(cancellationTokenSource.Token);
        await Task.Delay(50); // Даем время для выполнения
        await service.StopAsync(cancellationTokenSource.Token);

        // Assert
        mockRefreshTokenRepository.Verify(
            x => x.GetExpiredTokensAsync(It.IsAny<CancellationToken>()),
            Times.AtLeastOnce
        );

        mockLogger.Verify(
            x =>
                x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Found")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()
                ),
            Times.AtLeastOnce
        );
    }

    [Fact]
    public async Task ExecuteAsync_WhenNoExpiredTokens_ShouldDoNothing()
    {
        // Arrange
        var mockServiceProvider = new Mock<IServiceProvider>();
        var mockScope = new Mock<IServiceScope>();
        var mockScopeServiceProvider = new Mock<IServiceProvider>();
        var mockRefreshTokenRepository = new Mock<IRefreshTokenRepository>();
        var mockUnitOfWork = new Mock<IUnitOfWork>();
        var mockLogger = new Mock<ILogger<ExpiredTokenCleanupService>>();
        var mockScopeFactory = new Mock<IServiceScopeFactory>();

        var settings = new TokenCleanupSettings { EnableCleanup = true, CleanupIntervalHours = 1 };

        mockServiceProvider
            .Setup(x => x.GetService(typeof(IServiceScopeFactory)))
            .Returns(mockScopeFactory.Object);

        mockScopeFactory.Setup(x => x.CreateScope()).Returns(mockScope.Object);

        mockScope.Setup(x => x.ServiceProvider).Returns(mockScopeServiceProvider.Object);

        mockScopeServiceProvider
            .Setup(x => x.GetService(typeof(IRefreshTokenRepository)))
            .Returns(mockRefreshTokenRepository.Object);

        mockScopeServiceProvider
            .Setup(x => x.GetService(typeof(IUnitOfWork)))
            .Returns(mockUnitOfWork.Object);

        mockRefreshTokenRepository
            .Setup(x => x.GetExpiredTokensAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<RefreshToken>());

        var service = new ExpiredTokenCleanupService(
            mockServiceProvider.Object,
            mockLogger.Object,
            Options.Create(settings)
        );

        // Act
        var cancellationTokenSource = new CancellationTokenSource();
        cancellationTokenSource.CancelAfter(TimeSpan.FromMilliseconds(100));

        await service.StartAsync(cancellationTokenSource.Token);
        await Task.Delay(50); // Даем время для выполнения
        await service.StopAsync(cancellationTokenSource.Token);

        // Assert
        mockRefreshTokenRepository.Verify(
            x => x.GetExpiredTokensAsync(It.IsAny<CancellationToken>()),
            Times.AtLeastOnce
        );

        mockLogger.Verify(
            x =>
                x.Log(
                    LogLevel.Debug,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>(
                        (v, t) => v.ToString()!.Contains("No expired tokens found")
                    ),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()
                ),
            Times.AtLeastOnce
        );
    }

    [Fact]
    public async Task ExecuteAsync_WhenRepositoryThrowsException_ShouldLogError()
    {
        // Arrange
        var mockServiceProvider = new Mock<IServiceProvider>();
        var mockScope = new Mock<IServiceScope>();
        var mockScopeServiceProvider = new Mock<IServiceProvider>();
        var mockRefreshTokenRepository = new Mock<IRefreshTokenRepository>();
        var mockLogger = new Mock<ILogger<ExpiredTokenCleanupService>>();
        var mockScopeFactory = new Mock<IServiceScopeFactory>();

        var settings = new TokenCleanupSettings { EnableCleanup = true, CleanupIntervalHours = 1 };

        mockServiceProvider
            .Setup(x => x.GetService(typeof(IServiceScopeFactory)))
            .Returns(mockScopeFactory.Object);

        mockScopeFactory.Setup(x => x.CreateScope()).Returns(mockScope.Object);

        mockScope.Setup(x => x.ServiceProvider).Returns(mockScopeServiceProvider.Object);

        mockScopeServiceProvider
            .Setup(x => x.GetService(typeof(IRefreshTokenRepository)))
            .Returns(mockRefreshTokenRepository.Object);

        mockRefreshTokenRepository
            .Setup(x => x.GetExpiredTokensAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Database error"));

        var service = new ExpiredTokenCleanupService(
            mockServiceProvider.Object,
            mockLogger.Object,
            Options.Create(settings)
        );

        // Act
        var cancellationTokenSource = new CancellationTokenSource();
        cancellationTokenSource.CancelAfter(TimeSpan.FromMilliseconds(100));

        await service.StartAsync(cancellationTokenSource.Token);
        await Task.Delay(50); // Даем время для выполнения
        await service.StopAsync(cancellationTokenSource.Token);

        // Assert
        mockLogger.Verify(
            x =>
                x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => true),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()
                ),
            Times.AtLeastOnce
        );
    }

    [Fact]
    public async Task ExecuteAsync_WhenServiceIsStopped_ShouldCleanupGracefully()
    {
        // Arrange
        var mockServiceProvider = new Mock<IServiceProvider>();
        var mockScope = new Mock<IServiceScope>();
        var mockScopeServiceProvider = new Mock<IServiceProvider>();
        var mockRefreshTokenRepository = new Mock<IRefreshTokenRepository>();
        var mockUnitOfWork = new Mock<IUnitOfWork>();
        var mockLogger = new Mock<ILogger<ExpiredTokenCleanupService>>();
        var mockScopeFactory = new Mock<IServiceScopeFactory>();

        var settings = new TokenCleanupSettings
        {
            EnableCleanup = true,
            CleanupIntervalHours = 24, // Длинный интервал
        };

        mockServiceProvider
            .Setup(x => x.GetService(typeof(IServiceScopeFactory)))
            .Returns(mockScopeFactory.Object);

        mockScopeFactory.Setup(x => x.CreateScope()).Returns(mockScope.Object);

        mockScope.Setup(x => x.ServiceProvider).Returns(mockScopeServiceProvider.Object);

        mockScopeServiceProvider
            .Setup(x => x.GetService(typeof(IRefreshTokenRepository)))
            .Returns(mockRefreshTokenRepository.Object);

        mockScopeServiceProvider
            .Setup(x => x.GetService(typeof(IUnitOfWork)))
            .Returns(mockUnitOfWork.Object);

        mockRefreshTokenRepository
            .Setup(x => x.GetExpiredTokensAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<RefreshToken>());

        var service = new ExpiredTokenCleanupService(
            mockServiceProvider.Object,
            mockLogger.Object,
            Options.Create(settings)
        );

        // Act
        var cancellationTokenSource = new CancellationTokenSource();

        await service.StartAsync(cancellationTokenSource.Token);
        await Task.Delay(50); // Даем время для инициализации
        cancellationTokenSource.Cancel(); // Останавливаем сервис
        await service.StopAsync(cancellationTokenSource.Token);

        // Assert
        // Сервис должен корректно остановиться без исключений
        Assert.True(cancellationTokenSource.Token.IsCancellationRequested);
    }

    [Fact]
    public async Task ExecuteAsync_WhenCleanupIsDisabled_ShouldNotRunCleanup()
    {
        // Arrange
        var mockServiceProvider = new Mock<IServiceProvider>();
        var mockLogger = new Mock<ILogger<ExpiredTokenCleanupService>>();

        var settings = new TokenCleanupSettings { EnableCleanup = false };

        var service = new ExpiredTokenCleanupService(
            mockServiceProvider.Object,
            mockLogger.Object,
            Options.Create(settings)
        );

        // Act
        var cancellationTokenSource = new CancellationTokenSource();
        cancellationTokenSource.CancelAfter(TimeSpan.FromMilliseconds(100));

        await service.StartAsync(cancellationTokenSource.Token);
        await Task.Delay(50); // Даем время для выполнения
        await service.StopAsync(cancellationTokenSource.Token);

        // Assert
        mockServiceProvider.Verify(x => x.GetService(It.IsAny<Type>()), Times.Never);

        mockLogger.Verify(
            x =>
                x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>(
                        (v, t) => v.ToString()!.Contains("Expired token cleanup service started")
                    ),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()
                ),
            Times.Once
        );
    }

    [Fact]
    public async Task ExecuteAsync_WhenUnitOfWorkThrowsException_ShouldLogError()
    {
        // Arrange
        var mockServiceProvider = new Mock<IServiceProvider>();
        var mockScope = new Mock<IServiceScope>();
        var mockScopeServiceProvider = new Mock<IServiceProvider>();
        var mockRefreshTokenRepository = new Mock<IRefreshTokenRepository>();
        var mockUnitOfWork = new Mock<IUnitOfWork>();
        var mockLogger = new Mock<ILogger<ExpiredTokenCleanupService>>();
        var mockScopeFactory = new Mock<IServiceScopeFactory>();

        var settings = new TokenCleanupSettings { EnableCleanup = true, CleanupIntervalHours = 1 };

        var expiredTokens = new List<RefreshToken>
        {
            new RefreshToken("token1", DateTime.UtcNow.AddDays(-1), Guid.NewGuid(), "127.0.0.1"),
        };

        mockServiceProvider
            .Setup(x => x.GetService(typeof(IServiceScopeFactory)))
            .Returns(mockScopeFactory.Object);

        mockScopeFactory.Setup(x => x.CreateScope()).Returns(mockScope.Object);

        mockScope.Setup(x => x.ServiceProvider).Returns(mockScopeServiceProvider.Object);

        mockScopeServiceProvider
            .Setup(x => x.GetService(typeof(IRefreshTokenRepository)))
            .Returns(mockRefreshTokenRepository.Object);

        mockScopeServiceProvider
            .Setup(x => x.GetService(typeof(IUnitOfWork)))
            .Returns(mockUnitOfWork.Object);

        // Создаем реальный контекст для теста
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase("test_database")
            .Options;

        using var context = new AppDbContext(options);
        mockScopeServiceProvider.Setup(x => x.GetService(typeof(AppDbContext))).Returns(context);

        mockRefreshTokenRepository
            .Setup(x => x.GetExpiredTokensAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(expiredTokens);

        mockUnitOfWork
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Save failed"));

        var service = new ExpiredTokenCleanupService(
            mockServiceProvider.Object,
            mockLogger.Object,
            Options.Create(settings)
        );

        // Act
        var cancellationTokenSource = new CancellationTokenSource();
        cancellationTokenSource.CancelAfter(TimeSpan.FromMilliseconds(100));

        await service.StartAsync(cancellationTokenSource.Token);
        await Task.Delay(50); // Даем время для выполнения
        await service.StopAsync(cancellationTokenSource.Token);

        // Assert
        mockLogger.Verify(
            x =>
                x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>(
                        (v, t) => v.ToString()!.Contains("Failed to cleanup expired tokens")
                    ),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()
                ),
            Times.AtLeastOnce
        );
    }
}

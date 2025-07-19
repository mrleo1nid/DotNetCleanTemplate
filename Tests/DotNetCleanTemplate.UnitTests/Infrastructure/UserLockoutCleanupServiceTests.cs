using DotNetCleanTemplate.Domain.Repositories;
using DotNetCleanTemplate.Infrastructure.Configurations;
using DotNetCleanTemplate.Infrastructure.Services;
using DotNetCleanTemplate.UnitTests.Common;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;

namespace DotNetCleanTemplate.UnitTests.Infrastructure;

public class UserLockoutCleanupServiceTests : TestBase
{
    [Fact]
    public async Task ExecuteAsync_WhenExpiredLockoutsExist_ShouldRemoveThem()
    {
        // Arrange
        var mockServiceProvider = new Mock<IServiceProvider>();
        var mockScope = new Mock<IServiceScope>();
        var mockScopeServiceProvider = new Mock<IServiceProvider>();
        var mockUserLockoutRepository = new Mock<IUserLockoutRepository>();
        var mockLogger = new Mock<ILogger<UserLockoutCleanupService>>();
        var mockScopeFactory = new Mock<IServiceScopeFactory>();

        var settings = new UserLockoutCleanupSettings
        {
            Enabled = true,
            CleanupIntervalMinutes = 1,
        };

        mockServiceProvider
            .Setup(x => x.GetService(typeof(IServiceScopeFactory)))
            .Returns(mockScopeFactory.Object);

        mockScopeFactory.Setup(x => x.CreateScope()).Returns(mockScope.Object);

        mockScope.Setup(x => x.ServiceProvider).Returns(mockScopeServiceProvider.Object);

        mockScopeServiceProvider
            .Setup(x => x.GetService(typeof(IUserLockoutRepository)))
            .Returns(mockUserLockoutRepository.Object);

        mockUserLockoutRepository
            .Setup(x => x.ClearExpiredLockoutsAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var service = new UserLockoutCleanupService(
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
        mockUserLockoutRepository.Verify(
            x => x.ClearExpiredLockoutsAsync(It.IsAny<CancellationToken>()),
            Times.AtLeastOnce
        );
    }

    [Fact]
    public async Task ExecuteAsync_WhenNoExpiredLockouts_ShouldDoNothing()
    {
        // Arrange
        var mockServiceProvider = new Mock<IServiceProvider>();
        var mockScope = new Mock<IServiceScope>();
        var mockScopeServiceProvider = new Mock<IServiceProvider>();
        var mockUserLockoutRepository = new Mock<IUserLockoutRepository>();
        var mockLogger = new Mock<ILogger<UserLockoutCleanupService>>();
        var mockScopeFactory = new Mock<IServiceScopeFactory>();

        var settings = new UserLockoutCleanupSettings
        {
            Enabled = true,
            CleanupIntervalMinutes = 1,
        };

        mockServiceProvider
            .Setup(x => x.GetService(typeof(IServiceScopeFactory)))
            .Returns(mockScopeFactory.Object);

        mockScopeFactory.Setup(x => x.CreateScope()).Returns(mockScope.Object);

        mockScope.Setup(x => x.ServiceProvider).Returns(mockScopeServiceProvider.Object);

        mockScopeServiceProvider
            .Setup(x => x.GetService(typeof(IUserLockoutRepository)))
            .Returns(mockUserLockoutRepository.Object);

        mockUserLockoutRepository
            .Setup(x => x.ClearExpiredLockoutsAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var service = new UserLockoutCleanupService(
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
        mockUserLockoutRepository.Verify(
            x => x.ClearExpiredLockoutsAsync(It.IsAny<CancellationToken>()),
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
        var mockUserLockoutRepository = new Mock<IUserLockoutRepository>();
        var mockLogger = new Mock<ILogger<UserLockoutCleanupService>>();
        var mockScopeFactory = new Mock<IServiceScopeFactory>();

        var settings = new UserLockoutCleanupSettings
        {
            Enabled = true,
            CleanupIntervalMinutes = 1,
        };

        mockServiceProvider
            .Setup(x => x.GetService(typeof(IServiceScopeFactory)))
            .Returns(mockScopeFactory.Object);

        mockScopeFactory.Setup(x => x.CreateScope()).Returns(mockScope.Object);

        mockScope.Setup(x => x.ServiceProvider).Returns(mockScopeServiceProvider.Object);

        mockScopeServiceProvider
            .Setup(x => x.GetService(typeof(IUserLockoutRepository)))
            .Returns(mockUserLockoutRepository.Object);

        mockUserLockoutRepository
            .Setup(x => x.ClearExpiredLockoutsAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Database error"));

        var service = new UserLockoutCleanupService(
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
        var mockUserLockoutRepository = new Mock<IUserLockoutRepository>();
        var mockLogger = new Mock<ILogger<UserLockoutCleanupService>>();
        var mockScopeFactory = new Mock<IServiceScopeFactory>();

        var settings = new UserLockoutCleanupSettings
        {
            Enabled = true,
            CleanupIntervalMinutes = 60, // Длинный интервал
        };

        mockServiceProvider
            .Setup(x => x.GetService(typeof(IServiceScopeFactory)))
            .Returns(mockScopeFactory.Object);

        mockScopeFactory.Setup(x => x.CreateScope()).Returns(mockScope.Object);

        mockScope.Setup(x => x.ServiceProvider).Returns(mockScopeServiceProvider.Object);

        mockScopeServiceProvider
            .Setup(x => x.GetService(typeof(IUserLockoutRepository)))
            .Returns(mockUserLockoutRepository.Object);

        mockUserLockoutRepository
            .Setup(x => x.ClearExpiredLockoutsAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var service = new UserLockoutCleanupService(
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
    public async Task ExecuteAsync_WhenServiceIsDisabled_ShouldNotRunCleanup()
    {
        // Arrange
        var mockServiceProvider = new Mock<IServiceProvider>();
        var mockLogger = new Mock<ILogger<UserLockoutCleanupService>>();

        var settings = new UserLockoutCleanupSettings { Enabled = false };

        var service = new UserLockoutCleanupService(
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
                        (v, t) => v.ToString()!.Contains("UserLockoutCleanupService is disabled")
                    ),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()
                ),
            Times.Once
        );
    }

    [Fact]
    public async Task CleanupExpiredLockoutsAsync_WhenCalledDirectly_ShouldWorkCorrectly()
    {
        // Arrange
        var mockServiceProvider = new Mock<IServiceProvider>();
        var mockScope = new Mock<IServiceScope>();
        var mockScopeServiceProvider = new Mock<IServiceProvider>();
        var mockUserLockoutRepository = new Mock<IUserLockoutRepository>();
        var mockLogger = new Mock<ILogger<UserLockoutCleanupService>>();
        var mockScopeFactory = new Mock<IServiceScopeFactory>();

        var settings = new UserLockoutCleanupSettings
        {
            Enabled = true,
            CleanupIntervalMinutes = 1,
        };

        mockServiceProvider
            .Setup(x => x.GetService(typeof(IServiceScopeFactory)))
            .Returns(mockScopeFactory.Object);

        mockScopeFactory.Setup(x => x.CreateScope()).Returns(mockScope.Object);

        mockScope.Setup(x => x.ServiceProvider).Returns(mockScopeServiceProvider.Object);

        mockScopeServiceProvider
            .Setup(x => x.GetService(typeof(IUserLockoutRepository)))
            .Returns(mockUserLockoutRepository.Object);

        mockUserLockoutRepository
            .Setup(x => x.ClearExpiredLockoutsAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var service = new UserLockoutCleanupService(
            mockServiceProvider.Object,
            mockLogger.Object,
            Options.Create(settings)
        );

        // Act
        await service.CleanupExpiredLockoutsAsync(CancellationToken.None);

        // Assert
        mockUserLockoutRepository.Verify(
            x => x.ClearExpiredLockoutsAsync(It.IsAny<CancellationToken>()),
            Times.Once
        );

        mockLogger.Verify(
            x =>
                x.Log(
                    LogLevel.Debug,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>(
                        (v, t) => v.ToString()!.Contains("Successfully completed cleanup")
                    ),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()
                ),
            Times.Once
        );
    }

    [Fact]
    public async Task CleanupExpiredLockoutsAsync_WhenRepositoryThrowsException_ShouldLogError()
    {
        // Arrange
        var mockServiceProvider = new Mock<IServiceProvider>();
        var mockScope = new Mock<IServiceScope>();
        var mockScopeServiceProvider = new Mock<IServiceProvider>();
        var mockUserLockoutRepository = new Mock<IUserLockoutRepository>();
        var mockLogger = new Mock<ILogger<UserLockoutCleanupService>>();
        var mockScopeFactory = new Mock<IServiceScopeFactory>();

        var settings = new UserLockoutCleanupSettings
        {
            Enabled = true,
            CleanupIntervalMinutes = 1,
        };

        mockServiceProvider
            .Setup(x => x.GetService(typeof(IServiceScopeFactory)))
            .Returns(mockScopeFactory.Object);

        mockScopeFactory.Setup(x => x.CreateScope()).Returns(mockScope.Object);

        mockScope.Setup(x => x.ServiceProvider).Returns(mockScopeServiceProvider.Object);

        mockScopeServiceProvider
            .Setup(x => x.GetService(typeof(IUserLockoutRepository)))
            .Returns(mockUserLockoutRepository.Object);

        mockUserLockoutRepository
            .Setup(x => x.ClearExpiredLockoutsAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Database error"));

        var service = new UserLockoutCleanupService(
            mockServiceProvider.Object,
            mockLogger.Object,
            Options.Create(settings)
        );

        // Act
        await service.CleanupExpiredLockoutsAsync(CancellationToken.None);

        // Assert
        mockLogger.Verify(
            x =>
                x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>(
                        (v, t) => v.ToString()!.Contains("Failed to cleanup expired user lockouts")
                    ),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()
                ),
            Times.Once
        );
    }
}

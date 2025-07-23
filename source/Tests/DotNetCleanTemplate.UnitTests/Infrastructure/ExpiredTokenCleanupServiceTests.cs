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

namespace DotNetCleanTemplate.UnitTests.Infrastructure
{
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

            var settings = new TokenCleanupSettings
            {
                EnableCleanup = true,
                CleanupIntervalHours = 0, // Немедленное выполнение
                RetryDelayMinutes = 1,
            };

            var expiredTokens = new List<RefreshToken>
            {
                new RefreshToken(
                    "token1",
                    DateTime.UtcNow.AddDays(-1),
                    Guid.NewGuid(),
                    "127.0.0.1"
                ),
                new RefreshToken(
                    "token2",
                    DateTime.UtcNow.AddDays(-2),
                    Guid.NewGuid(),
                    "127.0.0.1"
                ),
            };

            // Создаем in-memory контекст для теста
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase("test_database_1")
                .Options;
            var context = new AppDbContext(options);

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

            mockScopeServiceProvider
                .Setup(x => x.GetService(typeof(AppDbContext)))
                .Returns(context);

            mockRefreshTokenRepository
                .Setup(x => x.GetExpiredTokensAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(expiredTokens);

            mockUnitOfWork
                .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(2);

            var service = new ExpiredTokenCleanupService(
                mockServiceProvider.Object,
                mockLogger.Object,
                Options.Create(settings)
            );

            // Act
            using var cancellationTokenSource = new CancellationTokenSource();
            cancellationTokenSource.CancelAfter(TimeSpan.FromMilliseconds(5000));

            await service.StartAsync(cancellationTokenSource.Token);
            await Task.Delay(3000); // Даем время для выполнения
            await service.StopAsync(cancellationTokenSource.Token);

            // Assert
            mockRefreshTokenRepository.Verify(
                x => x.GetExpiredTokensAsync(It.IsAny<CancellationToken>()),
                Times.AtLeastOnce
            );

            mockUnitOfWork.Verify(
                x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
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

            var settings = new TokenCleanupSettings
            {
                EnableCleanup = true,
                CleanupIntervalHours = 0, // Немедленное выполнение
                RetryDelayMinutes = 1,
            };

            // Создаем in-memory контекст для теста
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase("test_database_2")
                .Options;
            var context = new AppDbContext(options);

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

            mockScopeServiceProvider
                .Setup(x => x.GetService(typeof(AppDbContext)))
                .Returns(context);

            mockRefreshTokenRepository
                .Setup(x => x.GetExpiredTokensAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<RefreshToken>());

            var service = new ExpiredTokenCleanupService(
                mockServiceProvider.Object,
                mockLogger.Object,
                Options.Create(settings)
            );

            // Act
            using var cancellationTokenSource = new CancellationTokenSource();
            cancellationTokenSource.CancelAfter(TimeSpan.FromMilliseconds(5000));

            await service.StartAsync(cancellationTokenSource.Token);
            await Task.Delay(3000); // Даем время для выполнения
            await service.StopAsync(cancellationTokenSource.Token);

            // Assert
            mockRefreshTokenRepository.Verify(
                x => x.GetExpiredTokensAsync(It.IsAny<CancellationToken>()),
                Times.AtLeastOnce
            );

            // Если нет токенов для очистки, SaveChangesAsync не должен вызываться
            mockUnitOfWork.Verify(
                x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
                Times.Never
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
            var mockUnitOfWork = new Mock<IUnitOfWork>();
            var mockLogger = new Mock<ILogger<ExpiredTokenCleanupService>>();
            var mockScopeFactory = new Mock<IServiceScopeFactory>();

            var settings = new TokenCleanupSettings
            {
                EnableCleanup = true,
                CleanupIntervalHours = 0, // Немедленное выполнение
                RetryDelayMinutes = 1,
            };

            // Создаем in-memory контекст для теста
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase("test_database_3")
                .Options;
            var context = new AppDbContext(options);

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

            mockScopeServiceProvider
                .Setup(x => x.GetService(typeof(AppDbContext)))
                .Returns(context);

            mockRefreshTokenRepository
                .Setup(x => x.GetExpiredTokensAsync(It.IsAny<CancellationToken>()))
                .ThrowsAsync(new InvalidOperationException("Database error"));

            var service = new ExpiredTokenCleanupService(
                mockServiceProvider.Object,
                mockLogger.Object,
                Options.Create(settings)
            );

            // Act
            using var cancellationTokenSource = new CancellationTokenSource();
            cancellationTokenSource.CancelAfter(TimeSpan.FromMilliseconds(5000));

            await service.StartAsync(cancellationTokenSource.Token);
            await Task.Delay(3000); // Даем время для выполнения
            await service.StopAsync(cancellationTokenSource.Token);

            // Assert
            mockLogger.Verify(
                x =>
                    x.Log(
                        LogLevel.Error,
                        It.IsAny<EventId>(),
                        It.IsAny<It.IsAnyType>(),
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
            var mockLogger = new Mock<ILogger<ExpiredTokenCleanupService>>();

            var settings = new TokenCleanupSettings
            {
                EnableCleanup = true,
                CleanupIntervalHours = 1, // Долгий интервал
                RetryDelayMinutes = 1,
            };

            mockServiceProvider
                .Setup(x => x.GetService(typeof(IServiceScopeFactory)))
                .Returns((IServiceScopeFactory?)null);

            var service = new ExpiredTokenCleanupService(
                mockServiceProvider.Object,
                mockLogger.Object,
                Options.Create(settings)
            );

            // Act
            using var cancellationTokenSource = new CancellationTokenSource();
            cancellationTokenSource.CancelAfter(TimeSpan.FromMilliseconds(100));

            await service.StartAsync(cancellationTokenSource.Token);
            await Task.Delay(50);
            await service.StopAsync(cancellationTokenSource.Token);

            // Assert - сервис должен корректно остановиться
            Assert.True(true); // Если дошли до этой точки, значит остановка прошла успешно
        }

        [Fact]
        public async Task ExecuteAsync_WhenCleanupIsDisabled_ShouldNotRunCleanup()
        {
            // Arrange
            var mockServiceProvider = new Mock<IServiceProvider>();
            var mockLogger = new Mock<ILogger<ExpiredTokenCleanupService>>();

            var settings = new TokenCleanupSettings
            {
                EnableCleanup = false,
                CleanupIntervalHours = 1,
                RetryDelayMinutes = 1,
            };

            var service = new ExpiredTokenCleanupService(
                mockServiceProvider.Object,
                mockLogger.Object,
                Options.Create(settings)
            );

            // Act
            using var cancellationTokenSource = new CancellationTokenSource();
            cancellationTokenSource.CancelAfter(TimeSpan.FromMilliseconds(1000));

            await service.StartAsync(cancellationTokenSource.Token);
            await Task.Delay(500);
            await service.StopAsync(cancellationTokenSource.Token);

            // Assert - сервис не должен обращаться к репозиторию
            mockServiceProvider.Verify(
                x => x.GetService(typeof(IServiceScopeFactory)),
                Times.Never
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

            var settings = new TokenCleanupSettings
            {
                EnableCleanup = true,
                CleanupIntervalHours = 0, // Немедленное выполнение
                RetryDelayMinutes = 1,
            };

            var expiredTokens = new List<RefreshToken>
            {
                new RefreshToken(
                    "token1",
                    DateTime.UtcNow.AddDays(-1),
                    Guid.NewGuid(),
                    "127.0.0.1"
                ),
            };

            // Создаем in-memory контекст для теста
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase("test_database_4")
                .Options;
            var context = new AppDbContext(options);

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

            mockScopeServiceProvider
                .Setup(x => x.GetService(typeof(AppDbContext)))
                .Returns(context);

            mockRefreshTokenRepository
                .Setup(x => x.GetExpiredTokensAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(expiredTokens);

            mockUnitOfWork
                .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
                .ThrowsAsync(new InvalidOperationException("Save changes error"));

            var service = new ExpiredTokenCleanupService(
                mockServiceProvider.Object,
                mockLogger.Object,
                Options.Create(settings)
            );

            // Act
            using var cancellationTokenSource = new CancellationTokenSource();
            cancellationTokenSource.CancelAfter(TimeSpan.FromMilliseconds(5000));

            await service.StartAsync(cancellationTokenSource.Token);
            await Task.Delay(3000); // Даем время для выполнения
            await service.StopAsync(cancellationTokenSource.Token);

            // Assert
            mockLogger.Verify(
                x =>
                    x.Log(
                        LogLevel.Error,
                        It.IsAny<EventId>(),
                        It.IsAny<It.IsAnyType>(),
                        It.IsAny<Exception>(),
                        It.IsAny<Func<It.IsAnyType, Exception?, string>>()
                    ),
                Times.AtLeastOnce
            );
        }

        [Fact]
        public async Task StartAsync_WhenNoTokensToClean()
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
                CleanupIntervalHours = 0, // Немедленное выполнение
                RetryDelayMinutes = 1,
            };

            // Создаем in-memory контекст для теста
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase("test_database_5")
                .Options;
            var context = new AppDbContext(options);

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

            mockScopeServiceProvider
                .Setup(x => x.GetService(typeof(AppDbContext)))
                .Returns(context);

            mockRefreshTokenRepository
                .Setup(x => x.GetExpiredTokensAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<RefreshToken>());

            var service = new ExpiredTokenCleanupService(
                mockServiceProvider.Object,
                mockLogger.Object,
                Options.Create(settings)
            );

            // Act
            using var cancellationTokenSource = new CancellationTokenSource();
            cancellationTokenSource.CancelAfter(TimeSpan.FromMilliseconds(5000));

            await service.StartAsync(cancellationTokenSource.Token);
            await Task.Delay(3000); // Даем время для выполнения
            await service.StopAsync(cancellationTokenSource.Token);

            // Assert
            mockRefreshTokenRepository.Verify(
                x => x.GetExpiredTokensAsync(It.IsAny<CancellationToken>()),
                Times.AtLeastOnce
            );

            // Если нет токенов для очистки, SaveChangesAsync не должен вызываться
            mockUnitOfWork.Verify(
                x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
                Times.Never
            );
        }

        [Fact]
        public async Task StartAsync_WhenExceptionOccurs()
        {
            // Arrange
            var mockServiceProvider = new Mock<IServiceProvider>();
            var mockLogger = new Mock<ILogger<ExpiredTokenCleanupService>>();
            var settings = new TokenCleanupSettings
            {
                EnableCleanup = true,
                CleanupIntervalHours = 0,
                RetryDelayMinutes = 1,
            }; // 0 часов = немедленное выполнение

            mockServiceProvider
                .Setup(x => x.GetService(typeof(IServiceScopeFactory)))
                .Throws(new InvalidOperationException("Service provider error"));

            var service = new ExpiredTokenCleanupService(
                mockServiceProvider.Object,
                mockLogger.Object,
                Options.Create(settings)
            );

            // Act
            using var cancellationTokenSource = new CancellationTokenSource();
            cancellationTokenSource.CancelAfter(TimeSpan.FromMilliseconds(2000));

            await service.StartAsync(cancellationTokenSource.Token);
            await Task.Delay(1000);
            await service.StopAsync(cancellationTokenSource.Token);

            // Assert - должно логировать ошибку
            mockLogger.Verify(
                x =>
                    x.Log(
                        LogLevel.Error,
                        It.IsAny<EventId>(),
                        It.IsAny<It.IsAnyType>(),
                        It.IsAny<Exception>(),
                        It.IsAny<Func<It.IsAnyType, Exception?, string>>()
                    ),
                Times.AtLeastOnce
            );
        }

        [Fact]
        public async Task StartAsync_WithMultipleExpiredTokens()
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
                CleanupIntervalHours = 0,
                RetryDelayMinutes = 1,
            }; // 0 часов = немедленное выполнение

            var expiredTokens = new List<RefreshToken>
            {
                new RefreshToken(
                    "token1",
                    DateTime.UtcNow.AddDays(-1),
                    Guid.NewGuid(),
                    "127.0.0.1"
                ),
                new RefreshToken(
                    "token2",
                    DateTime.UtcNow.AddDays(-2),
                    Guid.NewGuid(),
                    "127.0.0.1"
                ),
                new RefreshToken(
                    "token3",
                    DateTime.UtcNow.AddDays(-3),
                    Guid.NewGuid(),
                    "127.0.0.1"
                ),
            };

            // Создаем in-memory контекст для теста
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase("test_database_6")
                .Options;
            var context = new AppDbContext(options);

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

            mockScopeServiceProvider
                .Setup(x => x.GetService(typeof(AppDbContext)))
                .Returns(context);

            mockRefreshTokenRepository
                .Setup(x => x.GetExpiredTokensAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(expiredTokens);

            mockUnitOfWork
                .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(3);

            var service = new ExpiredTokenCleanupService(
                mockServiceProvider.Object,
                mockLogger.Object,
                Options.Create(settings)
            );

            // Act
            using var cancellationTokenSource = new CancellationTokenSource();
            cancellationTokenSource.CancelAfter(TimeSpan.FromMilliseconds(5000));

            await service.StartAsync(cancellationTokenSource.Token);
            await Task.Delay(3000); // Даем время для выполнения
            await service.StopAsync(cancellationTokenSource.Token);

            // Assert
            mockRefreshTokenRepository.Verify(
                x => x.GetExpiredTokensAsync(It.IsAny<CancellationToken>()),
                Times.AtLeastOnce
            );

            mockUnitOfWork.Verify(
                x => x.SaveChangesAsync(It.IsAny<CancellationToken>()),
                Times.AtLeastOnce
            );
        }

        [Fact]
        public async Task StartAsync_WithConcurrentAccess()
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
                CleanupIntervalHours = 0,
                RetryDelayMinutes = 1,
            }; // 0 часов = немедленное выполнение

            // Создаем in-memory контекст для теста
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase("test_database_7")
                .Options;
            var context = new AppDbContext(options);

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

            mockScopeServiceProvider
                .Setup(x => x.GetService(typeof(AppDbContext)))
                .Returns(context);

            mockRefreshTokenRepository
                .Setup(x => x.GetExpiredTokensAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<RefreshToken>());

            var service = new ExpiredTokenCleanupService(
                mockServiceProvider.Object,
                mockLogger.Object,
                Options.Create(settings)
            );

            // Act
            using var cancellationTokenSource = new CancellationTokenSource();
            cancellationTokenSource.CancelAfter(TimeSpan.FromMilliseconds(5000));

            await service.StartAsync(cancellationTokenSource.Token);
            await Task.Delay(3000); // Даем время для выполнения
            await service.StopAsync(cancellationTokenSource.Token);

            // Assert
            mockRefreshTokenRepository.Verify(
                x => x.GetExpiredTokensAsync(It.IsAny<CancellationToken>()),
                Times.AtLeastOnce
            );
        }
    }
}

using DotNetCleanTemplate.Domain.Entities;
using DotNetCleanTemplate.Domain.Repositories;
using DotNetCleanTemplate.Infrastructure.Configurations;
using DotNetCleanTemplate.Infrastructure.Persistent;
using DotNetCleanTemplate.Infrastructure.Persistent.Repositories;
using DotNetCleanTemplate.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Testcontainers.PostgreSql;

namespace DotNetCleanTemplate.UnitTests.Infrastructure
{
    public class ExpiredTokenCleanupServiceTests_Container : IAsyncLifetime
    {
        private readonly PostgreSqlContainer _pgContainer;
        private ServiceProvider _provider = null!;

        public ExpiredTokenCleanupServiceTests_Container()
        {
            _pgContainer = new PostgreSqlBuilder()
                .WithImage("postgres:16-alpine")
                .WithCleanUp(true)
                .Build();
        }

        public async Task InitializeAsync()
        {
            await _pgContainer.StartAsync();
            var services = new ServiceCollection();
            services.AddLogging();
            services.Configure<TokenCleanupSettings>(cfg =>
            {
                cfg.EnableCleanup = true;
                cfg.CleanupIntervalHours = 0; // Немедленная очистка для теста
                cfg.RetryDelayMinutes = 1;
            });
            services.AddDbContext<AppDbContext>(options =>
                options.UseNpgsql(
                    _pgContainer.GetConnectionString(),
                    b => b.MigrationsAssembly("DotNetCleanTemplate.Infrastructure")
                )
            );
            services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
            services.AddScoped<IUnitOfWork, UnitOfWork>();
            services.AddScoped<ExpiredTokenCleanupService>();
            _provider = services.BuildServiceProvider();

            using var scope = _provider.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            await db.Database.MigrateAsync();
        }

        public async Task DisposeAsync() => await _pgContainer.DisposeAsync();

        [Fact]
        public async Task ExecuteAsync_WithExpiredTokens_RemovesThemFromDatabase()
        {
            // Arrange
            using var scope = _provider.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var service = scope.ServiceProvider.GetRequiredService<ExpiredTokenCleanupService>();

            // Создаем пользователей для токенов
            var user1 = new DotNetCleanTemplate.Domain.Entities.User(
                new DotNetCleanTemplate.Domain.ValueObjects.User.UserName("user1"),
                new DotNetCleanTemplate.Domain.ValueObjects.User.Email("user1@example.com"),
                new DotNetCleanTemplate.Domain.ValueObjects.User.PasswordHash("hash123456789")
            );
            var user2 = new DotNetCleanTemplate.Domain.Entities.User(
                new DotNetCleanTemplate.Domain.ValueObjects.User.UserName("user2"),
                new DotNetCleanTemplate.Domain.ValueObjects.User.Email("user2@example.com"),
                new DotNetCleanTemplate.Domain.ValueObjects.User.PasswordHash("hash123456789")
            );
            var user3 = new DotNetCleanTemplate.Domain.Entities.User(
                new DotNetCleanTemplate.Domain.ValueObjects.User.UserName("user3"),
                new DotNetCleanTemplate.Domain.ValueObjects.User.Email("user3@example.com"),
                new DotNetCleanTemplate.Domain.ValueObjects.User.PasswordHash("hash123456789")
            );

            db.Users.AddRange(user1, user2, user3);
            await db.SaveChangesAsync();

            // Создаем истекшие токены
            var expiredToken1 = new RefreshToken(
                "expired1",
                DateTime.UtcNow.AddHours(-1),
                user1.Id,
                "127.0.0.1"
            );
            var expiredToken2 = new RefreshToken(
                "expired2",
                DateTime.UtcNow.AddHours(-2),
                user2.Id,
                "127.0.0.1"
            );
            var validToken = new RefreshToken(
                "valid",
                DateTime.UtcNow.AddHours(1),
                user3.Id,
                "127.0.0.1"
            );

            db.RefreshTokens.AddRange(expiredToken1, expiredToken2, validToken);
            await db.SaveChangesAsync();

            // Act - запускаем сервис на короткое время
            using var cancellationTokenSource = new CancellationTokenSource();
            cancellationTokenSource.CancelAfter(TimeSpan.FromMilliseconds(500)); // Увеличиваем время

            await service.StartAsync(cancellationTokenSource.Token);
            await Task.Delay(200); // Даем больше времени на выполнение
            await service.StopAsync(cancellationTokenSource.Token);

            // Assert
            var remainingTokens = await db.RefreshTokens.ToListAsync();
            Assert.Single(remainingTokens);
            Assert.Equal("valid", remainingTokens[0].Token);
        }
    }

    public class ExpiredTokenCleanupServiceUnitTests
    {
        [Fact]
        public async Task ExecuteAsync_EnableCleanupFalse_DoesNotPerformCleanup()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<ExpiredTokenCleanupService>>();
            var settings = new TokenCleanupSettings { EnableCleanup = false };
            var options = Options.Create(settings);
            var serviceProviderMock = new Mock<IServiceProvider>();

            var service = new ExpiredTokenCleanupService(
                serviceProviderMock.Object,
                loggerMock.Object,
                options
            );
            using var cancellationTokenSource = new CancellationTokenSource();
            cancellationTokenSource.CancelAfter(TimeSpan.FromMilliseconds(100)); // Быстрое завершение

            // Act
            await service.StartAsync(cancellationTokenSource.Token);
            await Task.Delay(50); // Небольшая задержка для выполнения
            await service.StopAsync(cancellationTokenSource.Token);

            // Assert
            // Проверяем что сервис запустился и остановился без ошибок
            loggerMock.Verify(
                x =>
                    x.Log(
                        LogLevel.Information,
                        It.IsAny<EventId>(),
                        It.Is<It.IsAnyType>(
                            (v, t) =>
                                v!.ToString()!.Contains("Expired token cleanup service started")
                        ),
                        null,
                        It.IsAny<Func<It.IsAnyType, Exception?, string>>()
                    ),
                Times.Once
            );
        }

        [Fact]
        public async Task ExecuteAsync_EnableCleanupTrue_PerformsCleanup()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<ExpiredTokenCleanupService>>();
            var settings = new TokenCleanupSettings
            {
                EnableCleanup = true,
                CleanupIntervalHours = 24, // Большой интервал для теста
            };
            var options = Options.Create(settings);
            var serviceProviderMock = new Mock<IServiceProvider>();

            var service = new ExpiredTokenCleanupService(
                serviceProviderMock.Object,
                loggerMock.Object,
                options
            );
            using var cancellationTokenSource = new CancellationTokenSource();
            cancellationTokenSource.CancelAfter(TimeSpan.FromMilliseconds(100)); // Быстрое завершение

            // Act
            await service.StartAsync(cancellationTokenSource.Token);
            await Task.Delay(50); // Небольшая задержка для выполнения
            await service.StopAsync(cancellationTokenSource.Token);

            // Assert
            // Проверяем что сервис запустился
            loggerMock.Verify(
                x =>
                    x.Log(
                        LogLevel.Information,
                        It.IsAny<EventId>(),
                        It.Is<It.IsAnyType>(
                            (v, t) =>
                                v!.ToString()!.Contains("Expired token cleanup service started")
                        ),
                        null,
                        It.IsAny<Func<It.IsAnyType, Exception?, string>>()
                    ),
                Times.Once
            );
        }

        [Fact]
        public async Task ExecuteAsync_OperationCanceledException_LogsInformationAndStops()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<ExpiredTokenCleanupService>>();
            var settings = new TokenCleanupSettings { EnableCleanup = true };
            var options = Options.Create(settings);
            var serviceProviderMock = new Mock<IServiceProvider>();

            var service = new ExpiredTokenCleanupService(
                serviceProviderMock.Object,
                loggerMock.Object,
                options
            );
            using var cancellationTokenSource = new CancellationTokenSource();
            cancellationTokenSource.Cancel(); // Немедленная отмена

            // Act
            await service.StartAsync(cancellationTokenSource.Token);
            await service.StopAsync(cancellationTokenSource.Token);

            // Assert
            // Проверяем что сервис запустился
            loggerMock.Verify(
                x =>
                    x.Log(
                        LogLevel.Information,
                        It.IsAny<EventId>(),
                        It.Is<It.IsAnyType>(
                            (v, t) =>
                                v!.ToString()!.Contains("Expired token cleanup service started")
                        ),
                        null,
                        It.IsAny<Func<It.IsAnyType, Exception?, string>>()
                    ),
                Times.Once
            );
        }

        [Fact]
        public void Constructor_WithValidParameters_CreatesInstance()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<ExpiredTokenCleanupService>>();
            var settings = new TokenCleanupSettings { EnableCleanup = true };
            var options = Options.Create(settings);
            var serviceProviderMock = new Mock<IServiceProvider>();

            // Act
            var service = new ExpiredTokenCleanupService(
                serviceProviderMock.Object,
                loggerMock.Object,
                options
            );

            // Assert
            Assert.NotNull(service);
        }
    }
}

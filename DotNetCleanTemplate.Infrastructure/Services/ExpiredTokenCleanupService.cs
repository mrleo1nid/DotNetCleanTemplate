using DotNetCleanTemplate.Domain.Repositories;
using DotNetCleanTemplate.Infrastructure.Configurations;
using DotNetCleanTemplate.Infrastructure.Persistent;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace DotNetCleanTemplate.Infrastructure.Services
{
    public class ExpiredTokenCleanupService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<ExpiredTokenCleanupService> _logger;
        private readonly TokenCleanupSettings _settings;

        public ExpiredTokenCleanupService(
            IServiceProvider serviceProvider,
            ILogger<ExpiredTokenCleanupService> logger,
            IOptions<TokenCleanupSettings> settings
        )
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
            _settings = settings.Value;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Expired token cleanup service started");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    if (_settings.EnableCleanup)
                    {
                        await CleanupExpiredTokensAsync(stoppingToken);

                        // Ждем интервал только после выполнения очистки
                        if (_settings.CleanupIntervalHours > 0)
                        {
                            await Task.Delay(
                                TimeSpan.FromHours(_settings.CleanupIntervalHours),
                                stoppingToken
                            );
                        }
                        else
                        {
                            // Если интервал 0, выполняем немедленно и делаем небольшую паузу
                            await Task.Delay(TimeSpan.FromMilliseconds(100), stoppingToken);
                        }
                    }
                    else
                    {
                        // Если очистка отключена, ждем немного и проверяем снова
                        await Task.Delay(TimeSpan.FromSeconds(1), stoppingToken);
                    }
                }
                catch (OperationCanceledException ex)
                {
                    _logger.LogInformation(ex, "Expired token cleanup service stopped");
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(
                        ex,
                        "Error occurred during expired token cleanup: {Message}",
                        ex.Message
                    );
                    await Task.Delay(
                        TimeSpan.FromMinutes(_settings.RetryDelayMinutes),
                        stoppingToken
                    );
                }
            }
        }

        private async Task CleanupExpiredTokensAsync(CancellationToken cancellationToken)
        {
            using var scope = _serviceProvider.CreateScope();
            var refreshTokenRepository =
                scope.ServiceProvider.GetRequiredService<IRefreshTokenRepository>();
            var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            try
            {
                var expiredTokens = await refreshTokenRepository.GetExpiredTokensAsync(
                    cancellationToken
                );

                if (expiredTokens.Count == 0)
                {
                    _logger.LogDebug("No expired tokens found for cleanup");
                    return;
                }

                _logger.LogInformation(
                    "Found {Count} expired tokens to cleanup",
                    expiredTokens.Count
                );

                foreach (var token in expiredTokens)
                {
                    // Удаляем токен из контекста
                    context.RefreshTokens.Remove(token);
                }

                await unitOfWork.SaveChangesAsync(cancellationToken);
                _logger.LogInformation(
                    "Successfully cleaned up {Count} expired tokens",
                    expiredTokens.Count
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to cleanup expired tokens: {Message}", ex.Message);
            }
        }
    }
}

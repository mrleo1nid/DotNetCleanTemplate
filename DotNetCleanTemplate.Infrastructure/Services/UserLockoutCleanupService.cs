using DotNetCleanTemplate.Domain.Services;
using DotNetCleanTemplate.Infrastructure.Configurations;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace DotNetCleanTemplate.Infrastructure.Services;

public class UserLockoutCleanupService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<UserLockoutCleanupService> _logger;
    private readonly UserLockoutCleanupSettings _settings;

    public UserLockoutCleanupService(
        IServiceProvider serviceProvider,
        ILogger<UserLockoutCleanupService> logger,
        IOptions<UserLockoutCleanupSettings> settings
    )
    {
        _serviceProvider =
            serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _settings = settings?.Value ?? throw new ArgumentNullException(nameof(settings));
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (!_settings.Enabled)
        {
            _logger.LogInformation("UserLockoutCleanupService is disabled");
            return;
        }

        _logger.LogInformation(
            "UserLockoutCleanupService started with interval: {Interval} minutes",
            _settings.CleanupIntervalMinutes
        );

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await CleanupExpiredLockoutsAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error occurred during user lockout cleanup - continuing execution"
                );
            }

            await Task.Delay(TimeSpan.FromMinutes(_settings.CleanupIntervalMinutes), stoppingToken);
        }
    }

    public async Task CleanupExpiredLockoutsAsync(CancellationToken cancellationToken = default)
    {
        using var scope = _serviceProvider.CreateScope();
        var userLockoutRepository =
            scope.ServiceProvider.GetRequiredService<DotNetCleanTemplate.Domain.Repositories.IUserLockoutRepository>();

        try
        {
            _logger.LogDebug("Starting cleanup of expired user lockouts");

            await userLockoutRepository.ClearExpiredLockoutsAsync(cancellationToken);

            _logger.LogDebug("Successfully completed cleanup of expired user lockouts");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to cleanup expired user lockouts");
        }
    }
}

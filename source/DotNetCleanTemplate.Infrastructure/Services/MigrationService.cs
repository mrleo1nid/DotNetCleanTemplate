using DotNetCleanTemplate.Domain.Services;
using DotNetCleanTemplate.Infrastructure.Configurations;
using DotNetCleanTemplate.Infrastructure.Persistent;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace DotNetCleanTemplate.Infrastructure.Services
{
    public class MigrationService : IMigrationService
    {
        private readonly AppDbContext _dbContext;
        private readonly DatabaseSettings _dbSettings;
        private readonly ILogger<MigrationService> _logger;

        public MigrationService(
            AppDbContext dbContext,
            IOptions<DatabaseSettings> dbSettings,
            ILogger<MigrationService> logger
        )
        {
            _dbContext = dbContext;
            _dbSettings = dbSettings.Value;
            _logger = logger;
        }

        public async Task ApplyMigrationsIfEnabledAsync(
            CancellationToken cancellationToken = default
        )
        {
            if (_dbSettings.ApplyMigrationsOnStartup)
            {
                _logger.LogInformation("Applying database migrations...");
                await _dbContext.Database.MigrateAsync(cancellationToken);
                _logger.LogInformation("Database migrations applied successfully.");
            }
            else
            {
                _logger.LogInformation("Automatic migration is disabled by configuration.");
            }
        }
    }
}

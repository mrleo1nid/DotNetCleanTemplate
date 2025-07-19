using DotNetCleanTemplate.Domain.Decorators;
using DotNetCleanTemplate.Domain.Services;

namespace DotNetCleanTemplate.Application.Services
{
    public class ApplicationMigrationService : IMigrationServiceDecorator
    {
        private readonly IMigrationService _migrationService;

        public ApplicationMigrationService(IMigrationService migrationService)
        {
            _migrationService = migrationService;
        }

        public async Task ApplyMigrationsIfEnabledAsync(
            CancellationToken cancellationToken = default
        )
        {
            await _migrationService.ApplyMigrationsIfEnabledAsync(cancellationToken);
        }
    }
}

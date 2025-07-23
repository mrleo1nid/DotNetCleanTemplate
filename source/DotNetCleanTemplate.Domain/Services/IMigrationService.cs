namespace DotNetCleanTemplate.Domain.Services
{
    public interface IMigrationService
    {
        Task ApplyMigrationsIfEnabledAsync(CancellationToken cancellationToken = default);
    }
}

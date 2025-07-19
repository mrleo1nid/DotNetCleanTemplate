namespace DotNetCleanTemplate.Domain.Decorators
{
    public interface IMigrationServiceDecorator
    {
        Task ApplyMigrationsIfEnabledAsync(CancellationToken cancellationToken = default);
    }
}

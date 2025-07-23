namespace DotNetCleanTemplate.Domain.Services;

public interface IUserLockoutCleanupService
{
    Task CleanupExpiredLockoutsAsync(CancellationToken cancellationToken = default);
}

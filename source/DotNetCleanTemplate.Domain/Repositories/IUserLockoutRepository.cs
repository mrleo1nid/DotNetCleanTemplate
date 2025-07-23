using DotNetCleanTemplate.Domain.Entities;

namespace DotNetCleanTemplate.Domain.Repositories;

public interface IUserLockoutRepository : IRepository
{
    Task<UserLockout?> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<bool> IsUserLockedAsync(Guid userId, CancellationToken cancellationToken = default);
    Task ClearExpiredLockoutsAsync(CancellationToken cancellationToken = default);
    Task<UserLockout> AddOrUpdateAsync(
        UserLockout lockout,
        CancellationToken cancellationToken = default
    );
    Task<UserLockout> IncrementFailedAttemptsAsync(
        Guid userId,
        int maxFailedAttempts,
        int lockoutDurationMinutes,
        CancellationToken cancellationToken = default
    );
}

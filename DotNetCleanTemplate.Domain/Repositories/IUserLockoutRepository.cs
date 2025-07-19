using DotNetCleanTemplate.Domain.Entities;

namespace DotNetCleanTemplate.Domain.Repositories;

public interface IUserLockoutRepository : IRepository
{
    Task<UserLockout?> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<bool> IsUserLockedAsync(Guid userId, CancellationToken cancellationToken = default);
    Task ClearExpiredLockoutsAsync(CancellationToken cancellationToken = default);
}

using DotNetCleanTemplate.Domain.Entities;
using DotNetCleanTemplate.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace DotNetCleanTemplate.Infrastructure.Persistent.Repositories;

public class UserLockoutRepository : BaseRepository, IUserLockoutRepository
{
    public UserLockoutRepository(AppDbContext context)
        : base(context) { }

    public async Task<UserLockout?> GetByUserIdAsync(
        Guid userId,
        CancellationToken cancellationToken = default
    )
    {
        return await _context
            .Set<UserLockout>()
            .FirstOrDefaultAsync(x => x.UserId == userId, cancellationToken);
    }

    public async Task<bool> IsUserLockedAsync(
        Guid userId,
        CancellationToken cancellationToken = default
    )
    {
        var lockout = await GetByUserIdAsync(userId, cancellationToken);
        return lockout?.IsLocked ?? false;
    }

    public async Task ClearExpiredLockoutsAsync(CancellationToken cancellationToken = default)
    {
        var expiredLockouts = await _context
            .Set<UserLockout>()
            .Where(x => x.LockoutEnd <= DateTime.UtcNow)
            .ToListAsync(cancellationToken);

        foreach (var lockout in expiredLockouts)
        {
            lockout.ClearLockout();
        }

        await _context.SaveChangesAsync(cancellationToken);
    }
}

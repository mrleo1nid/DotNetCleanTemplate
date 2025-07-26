using DotNetCleanTemplate.Domain.Entities;
using DotNetCleanTemplate.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace DotNetCleanTemplate.Infrastructure.Persistent.Repositories;

public class UserLockoutRepository : BaseRepository<UserLockout>, IUserLockoutRepository
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
        // SaveChangesAsync будет вызван через UnitOfWork
    }

    public async Task<UserLockout> AddOrUpdateAsync(
        UserLockout lockout,
        CancellationToken cancellationToken = default
    )
    {
        return await AddOrUpdateAsync(lockout, x => x.UserId == lockout.UserId);
    }

    public async Task<UserLockout> IncrementFailedAttemptsAsync(
        Guid userId,
        int maxFailedAttempts,
        int lockoutDurationMinutes,
        CancellationToken cancellationToken = default
    )
    {
        var lockoutEnd = DateTime.UtcNow.AddMinutes(lockoutDurationMinutes);
        var lockout = await _context
            .Set<UserLockout>()
            .FirstOrDefaultAsync(x => x.UserId == userId, cancellationToken);
        if (lockout == null)
        {
            lockout = new UserLockout(userId, lockoutEnd, 1);
            await _context.Set<UserLockout>().AddAsync(lockout, cancellationToken);
        }
        else
        {
            lockout.IncrementFailedAttempts();
            if (lockout.FailedAttempts >= maxFailedAttempts)
            {
                lockout.ExtendLockout(lockoutEnd);
            }
        }
        // SaveChangesAsync будет вызван через UnitOfWork
        return lockout;
    }
}

using DotNetCleanTemplate.Domain.Common;

namespace DotNetCleanTemplate.Domain.Entities;

public class UserLockout : Entity<Guid>
{
    public Guid UserId { get; private set; }
    public User User { get; private set; } = null!;
    public DateTime LockoutEnd { get; private set; }
    public int FailedAttempts { get; private set; }
    public bool IsLocked => LockoutEnd > DateTime.UtcNow;

    private UserLockout() { }

    public UserLockout(Guid userId, DateTime lockoutEnd, int failedAttempts = 0)
    {
        Id = Guid.NewGuid();
        UserId = userId;
        LockoutEnd = lockoutEnd;
        FailedAttempts = failedAttempts;
        CreatedAt = DateTime.UtcNow;
    }

    public void IncrementFailedAttempts()
    {
        FailedAttempts++;
        UpdatedAt = DateTime.UtcNow;
    }

    public void ResetFailedAttempts()
    {
        FailedAttempts = 0;
        UpdatedAt = DateTime.UtcNow;
    }

    public void ExtendLockout(DateTime newLockoutEnd)
    {
        LockoutEnd = newLockoutEnd;
        UpdatedAt = DateTime.UtcNow;
    }

    public void ClearLockout()
    {
        LockoutEnd = DateTime.UtcNow.AddMinutes(-1);
        FailedAttempts = 0;
        UpdatedAt = DateTime.UtcNow;
    }
}

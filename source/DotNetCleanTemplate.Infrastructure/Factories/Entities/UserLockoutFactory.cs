using DotNetCleanTemplate.Domain.Factories.Entities;
using UserLockoutEntity = DotNetCleanTemplate.Domain.Entities.UserLockout;

namespace DotNetCleanTemplate.Infrastructure.Factories.Entities
{
    public class UserLockoutFactory : IUserLockoutFactory
    {
        public UserLockoutEntity Create(Guid userId, DateTime lockoutEnd, int failedAttempts)
        {
            return new UserLockoutEntity(userId, lockoutEnd, failedAttempts);
        }
    }
}

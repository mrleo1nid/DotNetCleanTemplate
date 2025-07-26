using UserLockoutEntity = DotNetCleanTemplate.Domain.Entities.UserLockout;

namespace DotNetCleanTemplate.Domain.Factories.Entities
{
    public interface IUserLockoutFactory : IFactory
    {
        UserLockoutEntity Create(Guid userId, DateTime lockoutEnd, int failedAttempts);
    }
}

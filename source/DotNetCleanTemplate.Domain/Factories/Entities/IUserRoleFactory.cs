using UserRoleEntity = DotNetCleanTemplate.Domain.Entities.UserRole;

namespace DotNetCleanTemplate.Domain.Factories.Entities
{
    public interface IUserRoleFactory : IFactory
    {
        UserRoleEntity Create(Guid userId, Guid roleId);
    }
}

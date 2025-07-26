using RoleEntity = DotNetCleanTemplate.Domain.Entities.Role;

namespace DotNetCleanTemplate.Domain.Factories.Entities
{
    public interface IRoleFactory : IFactory
    {
        RoleEntity Create(string roleName);
    }
}

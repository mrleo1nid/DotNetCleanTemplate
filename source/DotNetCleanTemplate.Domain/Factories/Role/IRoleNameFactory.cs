using DotNetCleanTemplate.Domain.ValueObjects.Role;

namespace DotNetCleanTemplate.Domain.Factories.Role
{
    public interface IRoleNameFactory : IFactory
    {
        RoleName Create(string roleName);
    }
}

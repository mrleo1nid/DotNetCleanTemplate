using DotNetCleanTemplate.Domain.Factories.Role;
using DotNetCleanTemplate.Domain.ValueObjects.Role;

namespace DotNetCleanTemplate.Infrastructure.Factories.Role
{
    public class RoleNameFactory : IRoleNameFactory
    {
        public RoleName Create(string roleName)
        {
            return new RoleName(roleName);
        }
    }
}

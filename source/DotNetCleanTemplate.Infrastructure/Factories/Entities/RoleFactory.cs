using DotNetCleanTemplate.Domain.Factories.Entities;
using DotNetCleanTemplate.Domain.Factories.Role;
using RoleEntity = DotNetCleanTemplate.Domain.Entities.Role;

namespace DotNetCleanTemplate.Infrastructure.Factories.Entities
{
    public class RoleFactory : IRoleFactory
    {
        private readonly IRoleNameFactory _roleNameFactory;

        public RoleFactory(IRoleNameFactory roleNameFactory)
        {
            _roleNameFactory = roleNameFactory;
        }

        public RoleEntity Create(string roleName)
        {
            var roleNameValue = _roleNameFactory.Create(roleName);
            return new RoleEntity(roleNameValue);
        }
    }
}

using DotNetCleanTemplate.Domain.Common;
using DotNetCleanTemplate.Domain.ValueObjects.Role;

namespace DotNetCleanTemplate.Domain.Entities
{
    public class Role : Entity<Guid>
    {
        public RoleName Name { get; private set; }
        private readonly List<UserRole> _userRoles = new();

        public IReadOnlyCollection<UserRole> UserRoles => _userRoles.AsReadOnly();

        public Role(RoleName name)
        {
            Id = Guid.NewGuid();
            Name = name;
        }

        // Для EF Core
#pragma warning disable CS8618
        protected Role() { }
#pragma warning restore CS8618
    }
}

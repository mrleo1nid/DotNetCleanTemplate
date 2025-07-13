using DotNetCleanTemplate.Domain.Common;
using DotNetCleanTemplate.Domain.ValueObjects.User;

namespace DotNetCleanTemplate.Domain.Entities
{
    public class User : Entity<Guid>
    {
        public UserName Name { get; private set; }
        public Email Email { get; private set; }
        public PasswordHash PasswordHash { get; private set; }
        private readonly List<UserRole> _userRoles = new();

        public IReadOnlyCollection<UserRole> UserRoles => _userRoles.AsReadOnly();

        public User(UserName name, Email email, PasswordHash passwordHash)
        {
            Id = Guid.NewGuid();
            Name = name;
            Email = email;
            PasswordHash = passwordHash;
        }

        public void AssignRole(Role role)
        {
            if (_userRoles.Any(ur => ur.RoleId == role.Id))
                return;

            var userRole = new UserRole(this, role);
            _userRoles.Add(userRole);
        }

        public void RemoveRole(Role role)
        {
            var userRole = _userRoles.FirstOrDefault(ur => ur.RoleId == role.Id);
            if (userRole != null)
                _userRoles.Remove(userRole);
        }

        // Для EF Core
#pragma warning disable CS8618
        protected User() { }
#pragma warning restore CS8618
    }
}

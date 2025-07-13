using DotNetCleanTemplate.Domain.Common;
using DotNetCleanTemplate.Domain.ValueObjects.User;

namespace DotNetCleanTemplate.Domain.Entities
{
    public class User : Entity<Guid>
    {
        public UserName Name { get; private set; }
        public Email Email { get; private set; }
        public PasswordHash PasswordHash { get; private set; }
        private readonly List<Guid> _roleIds = new();
        public IReadOnlyCollection<Guid> RoleIds => _roleIds.AsReadOnly();

        public User(
            UserName name,
            Email email,
            PasswordHash passwordHash,
            IEnumerable<Guid>? roleIds = null
        )
        {
            Id = Guid.NewGuid();
            Name = name;
            Email = email;
            PasswordHash = passwordHash;
            if (roleIds != null)
                _roleIds.AddRange(roleIds);
        }

        public void AddRole(Guid roleId)
        {
            if (!_roleIds.Contains(roleId))
                _roleIds.Add(roleId);
        }

        public void RemoveRole(Guid roleId)
        {
            _roleIds.Remove(roleId);
        }

        // Для EF Core
#pragma warning disable CS8618
        protected User() { }
#pragma warning restore CS8618
    }
}

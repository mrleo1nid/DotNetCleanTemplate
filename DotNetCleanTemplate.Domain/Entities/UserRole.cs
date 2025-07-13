using DotNetCleanTemplate.Domain.Common;

namespace DotNetCleanTemplate.Domain.Entities
{
    public class UserRole : Entity<Guid>
    {
        public Guid UserId { get; private set; }
        public Guid RoleId { get; private set; }

        // Навигационные свойства
        public User User { get; private set; }
        public Role Role { get; private set; }

        public UserRole(User user, Role role)
        {
            UserId = user.Id;
            RoleId = role.Id;
            User = user;
            Role = role;
        }

        // Для EF Core
#pragma warning disable CS8618
        protected UserRole() { }
#pragma warning restore CS8618
    }
}

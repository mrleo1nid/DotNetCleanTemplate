using DotNetCleanTemplate.Domain.Factories.Entities;
using RoleEntity = DotNetCleanTemplate.Domain.Entities.Role;
using UserEntity = DotNetCleanTemplate.Domain.Entities.User;
using UserRoleEntity = DotNetCleanTemplate.Domain.Entities.UserRole;

namespace DotNetCleanTemplate.Infrastructure.Factories.Entities
{
    public class UserRoleFactory : IUserRoleFactory
    {
        public UserRoleEntity Create(Guid userId, Guid roleId)
        {
            // Создаем временные объекты для связи
            // В реальном использовании эти объекты должны быть получены из репозитория
            var user = new UserEntity(
                new DotNetCleanTemplate.Domain.ValueObjects.User.UserName("temp"),
                new DotNetCleanTemplate.Domain.ValueObjects.User.Email("temp@temp.com"),
                new DotNetCleanTemplate.Domain.ValueObjects.User.PasswordHash("temp")
            );
            user.GetType().GetProperty("Id")?.SetValue(user, userId);

            var role = new RoleEntity(
                new DotNetCleanTemplate.Domain.ValueObjects.Role.RoleName("temp")
            );
            role.GetType().GetProperty("Id")?.SetValue(role, roleId);

            return new UserRoleEntity(user, role);
        }
    }
}

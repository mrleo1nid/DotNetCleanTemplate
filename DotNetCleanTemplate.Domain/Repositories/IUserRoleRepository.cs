using DotNetCleanTemplate.Domain.Entities;

namespace DotNetCleanTemplate.Domain.Repositories
{
    public interface IUserRoleRepository : IRepository
    {
        Task<IEnumerable<UserRole>> GetByUserIdAsync(
            Guid userId,
            CancellationToken cancellationToken = default
        );
        Task<IEnumerable<UserRole>> GetByRoleIdAsync(
            Guid roleId,
            CancellationToken cancellationToken = default
        );
        Task<User?> GetUserWithRolesAsync(
            Guid userId,
            CancellationToken cancellationToken = default
        );
    }
}

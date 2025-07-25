using DotNetCleanTemplate.Domain.Entities;

namespace DotNetCleanTemplate.Domain.Repositories
{
    public interface IUserRepository : IRepository
    {
        Task<User?> FindByEmailAsync(string email, CancellationToken cancellationToken = default);
        Task<User?> FindByUserNameAsync(
            string userName,
            CancellationToken cancellationToken = default
        );
        Task<User?> GetUserWithRolesAsync(
            Guid userId,
            CancellationToken cancellationToken = default
        );
        Task<List<User>> GetAllUsersWithRolesAsync(CancellationToken cancellationToken = default);
        Task<(List<User> Users, int TotalCount)> GetUsersWithRolesPaginatedAsync(
            int page,
            int pageSize,
            CancellationToken cancellationToken = default
        );
        Task<IEnumerable<User>> GetUsersByRoleAsync(
            Guid roleId,
            CancellationToken cancellationToken = default
        );
    }
}

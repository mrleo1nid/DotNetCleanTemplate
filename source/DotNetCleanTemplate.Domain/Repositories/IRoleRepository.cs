using DotNetCleanTemplate.Domain.Entities;

namespace DotNetCleanTemplate.Domain.Repositories
{
    public interface IRoleRepository : IRepository
    {
        Task<Role?> FindByNameAsync(string name, CancellationToken cancellationToken = default);
        Task<User?> GetUserWithRolesAsync(
            Guid userId,
            CancellationToken cancellationToken = default
        );
        Task<(List<Role> Roles, int TotalCount)> GetRolesPaginatedAsync(
            int page,
            int pageSize,
            CancellationToken cancellationToken = default
        );
    }
}

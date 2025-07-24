using DotNetCleanTemplate.Domain.Entities;
using DotNetCleanTemplate.Shared.Common;
using MediatR;

namespace DotNetCleanTemplate.Application.Interfaces
{
    public interface IRoleService
    {
        Task<Result<Role>> FindByNameAsync(
            string name,
            CancellationToken cancellationToken = default
        );
        Task<Result<Role>> CreateRoleAsync(
            Role role,
            CancellationToken cancellationToken = default
        );
        Task<Result<Unit>> DeleteRoleAsync(
            Guid roleId,
            CancellationToken cancellationToken = default
        );
        Task<Result<List<Role>>> GetAllRolesAsync(CancellationToken cancellationToken = default);
        Task<Result<(List<Role> Roles, int TotalCount)>> GetRolesPaginatedAsync(
            int page,
            int pageSize,
            CancellationToken cancellationToken = default
        );
    }
}

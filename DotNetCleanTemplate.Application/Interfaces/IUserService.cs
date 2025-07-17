using DotNetCleanTemplate.Domain.Entities;
using DotNetCleanTemplate.Shared.Common;
using MediatR;

namespace DotNetCleanTemplate.Application.Interfaces
{
    public interface IUserService
    {
        Task<Result<User>> FindByEmailAsync(
            string email,
            CancellationToken cancellationToken = default
        );
        Task<Result<User>> CreateUserAsync(
            User user,
            CancellationToken cancellationToken = default
        );
        Task<Result<List<User>>> GetAllUsersWithRolesAsync(
            CancellationToken cancellationToken = default
        );
        Task<Result<Unit>> AssignRoleToUserAsync(
            Guid userId,
            Guid roleId,
            CancellationToken cancellationToken = default
        );
    }
}

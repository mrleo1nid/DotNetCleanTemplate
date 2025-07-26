using DotNetCleanTemplate.Shared.Common;
using DotNetCleanTemplate.Shared.DTOs;
using MediatR;

namespace DotNetCleanTemplate.Client.Services;

public interface IUserService
{
    Task<Result<PaginatedResultDto<UserWithRolesDto>>> GetUsersPaginatedAsync(
        int page,
        int pageSize
    );
    Task<Result<UserWithRolesDto>> GetUserWithRolesAsync(Guid userId);
    Task<Result<UserWithRolesDto>> CreateUserAsync(CreateUserDto createUserDto);
    Task<Result<Unit>> DeleteUserAsync(Guid userId);
    Task<Result<Unit>> ChangeUserPasswordAsync(
        Guid userId,
        ChangeUserPasswordDto changePasswordDto
    );
    Task<Result<Unit>> AssignRoleToUserAsync(Guid userId, AssignRoleToUserDto assignRoleDto);
    Task<Result<Unit>> RemoveRoleFromUserAsync(Guid userId, string roleName);
}

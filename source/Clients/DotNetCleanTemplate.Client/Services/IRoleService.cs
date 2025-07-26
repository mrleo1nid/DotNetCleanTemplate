using DotNetCleanTemplate.Shared.Common;
using DotNetCleanTemplate.Shared.DTOs;
using MediatR;

namespace DotNetCleanTemplate.Client.Services;

public interface IRoleService
{
    Task<Result<PaginatedResultDto<RoleDto>>> GetRolesPaginatedAsync(int page, int pageSize);
    Task<Result<List<RoleDto>>> GetAllRolesAsync();
    Task<Result<RoleDto>> CreateRoleAsync(CreateRoleDto createRoleDto);
    Task<Result<Unit>> DeleteRoleAsync(Guid roleId);
}

using DotNetCleanTemplate.Shared.Common;
using DotNetCleanTemplate.Shared.DTOs;
using MediatR;

namespace DotNetCleanTemplate.WebClient.Services;

public interface IUserManagementService
{
    Task<Result<PaginatedResultDto<UserWithRolesDto>>> LoadUsersAsync(int page, int pageSize);
    Task<Result<Unit>> DeleteUserAsync(Guid userId);
    Task<bool> ConfirmUserDeletionAsync();
    Task<bool> OpenCreateUserDialogAsync();
    Task<bool> OpenChangePasswordDialogAsync(Guid userId, string userName);
    Task<bool> OpenManageRolesDialogAsync(Guid userId, string userName);
}

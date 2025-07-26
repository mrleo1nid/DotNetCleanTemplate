using DotNetCleanTemplate.Client.Services;
using DotNetCleanTemplate.Shared.Common;
using DotNetCleanTemplate.Shared.DTOs;
using DotNetCleanTemplate.WebClient.Components;
using MediatR;
using MudBlazor;

namespace DotNetCleanTemplate.WebClient.Services;

public class UserManagementService : IUserManagementService
{
    private readonly IUserService _userService;
    private readonly IDialogService _dialogService;
    private readonly ISnackbar _snackbar;

    public UserManagementService(
        IUserService userService,
        IDialogService dialogService,
        ISnackbar snackbar
    )
    {
        _userService = userService;
        _dialogService = dialogService;
        _snackbar = snackbar;
    }

    public async Task<Result<PaginatedResultDto<UserWithRolesDto>>> LoadUsersAsync(
        int page,
        int pageSize
    )
    {
        try
        {
            var result = await _userService.GetUsersPaginatedAsync(page, pageSize);

            if (result.IsSuccess && result.Value != null)
            {
                _snackbar.Add("Пользователи загружены успешно", Severity.Success);
                return result;
            }
            else
            {
                var errorMessage =
                    result.Errors.FirstOrDefault()?.Message ?? "Ошибка при загрузке пользователей";
                _snackbar.Add(errorMessage, Severity.Error);
                return result;
            }
        }
        catch (Exception ex)
        {
            _snackbar.Add($"Ошибка: {ex.Message}", Severity.Error);
            return Result<PaginatedResultDto<UserWithRolesDto>>.Failure("LoadUsers", ex.Message);
        }
    }

    public async Task<Result<Unit>> DeleteUserAsync(Guid userId)
    {
        try
        {
            var result = await _userService.DeleteUserAsync(userId);

            if (result.IsSuccess)
            {
                _snackbar.Add("Пользователь успешно удален", Severity.Success);
                return Result<Unit>.Success();
            }
            else
            {
                var errorMessage =
                    result.Errors.FirstOrDefault()?.Message ?? "Ошибка при удалении пользователя";
                _snackbar.Add(errorMessage, Severity.Error);
                return result;
            }
        }
        catch (Exception ex)
        {
            _snackbar.Add($"Ошибка: {ex.Message}", Severity.Error);
            return Result<Unit>.Failure("DeleteUser", ex.Message);
        }
    }

    public async Task<bool> ConfirmUserDeletionAsync()
    {
        var parameters = new DialogParameters
        {
            ["ContentText"] = "Вы уверены, что хотите удалить этого пользователя?",
            ["ButtonText"] = "Удалить",
            ["Color"] = Color.Error,
        };

        var options = new DialogOptions() { CloseButton = true, MaxWidth = MaxWidth.ExtraSmall };
        var dialog = await _dialogService.ShowAsync<ConfirmDialog>(
            "Подтверждение удаления",
            parameters,
            options
        );
        var result = await dialog.Result;

        return result is not null && !result.Canceled;
    }

    public async Task<bool> OpenCreateUserDialogAsync()
    {
        var parameters = new DialogParameters();
        var dialog = await _dialogService.ShowAsync<CreateUserDialog>(
            "Создать пользователя",
            parameters
        );
        var result = await dialog.Result;

        return result is not null && !result.Canceled;
    }

    public async Task<bool> OpenChangePasswordDialogAsync(Guid userId, string userName)
    {
        var parameters = new DialogParameters { ["UserId"] = userId, ["UserName"] = userName };

        var dialog = await _dialogService.ShowAsync<ChangePasswordDialog>(
            "Изменить пароль",
            parameters
        );
        var result = await dialog.Result;

        if (result is not null && !result.Canceled)
        {
            _snackbar.Add($"Пароль для пользователя {userName} успешно изменен", Severity.Success);
            return true;
        }

        return false;
    }

    public async Task<bool> OpenManageRolesDialogAsync(Guid userId, string userName)
    {
        var parameters = new DialogParameters { ["UserId"] = userId, ["UserName"] = userName };

        var dialog = await _dialogService.ShowAsync<ManageUserRolesDialog>(
            "Управление ролями пользователя",
            parameters
        );
        await dialog.Result;
        return true;
    }
}

using System.Net.Http.Json;
using System.Text.Json;
using DotNetCleanTemplate.Shared.Common;
using DotNetCleanTemplate.Shared.DTOs;
using MediatR;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace DotNetCleanTemplate.Client.Services;

public class UserService : IUserService
{
    private const string HttpErrorCode = "HttpError";
    private const string NetworkErrorCode = "NetworkError";
    private const string UnknownErrorCode = "Unknown";
    private const string NetworkErrorMessage = "Ошибка сети";

    private readonly HttpClient _httpClient;
    private readonly ILogger<UserService> _logger;
    private readonly JsonSerializerOptions _jsonOptions;

    public UserService(
        HttpClient httpClient,
        ILogger<UserService> logger,
        IOptions<JsonSerializerOptions> jsonOptions
    )
    {
        _httpClient = httpClient;
        _logger = logger;
        _jsonOptions = jsonOptions.Value;
    }

    public async Task<Result<PaginatedResultDto<UserWithRolesDto>>> GetUsersPaginatedAsync(
        int page,
        int pageSize
    )
    {
        try
        {
            var response = await _httpClient.GetAsync(
                $"/administration/users/paginated?page={page}&pageSize={pageSize}"
            );

            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<
                    Result<PaginatedResultDto<UserWithRolesDto>>
                >(_jsonOptions);
                return result
                    ?? Result<PaginatedResultDto<UserWithRolesDto>>.Failure(
                        UnknownErrorCode,
                        "Не удалось десериализовать ответ"
                    );
            }

            _logger.LogWarning(
                "Ошибка при получении пользователей: {StatusCode}",
                response.StatusCode
            );
            return Result<PaginatedResultDto<UserWithRolesDto>>.Failure(
                HttpErrorCode,
                $"HTTP ошибка: {response.StatusCode}"
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при получении пользователей");
            return Result<PaginatedResultDto<UserWithRolesDto>>.Failure(
                NetworkErrorCode,
                NetworkErrorMessage
            );
        }
    }

    public async Task<Result<UserWithRolesDto>> GetUserWithRolesAsync(Guid userId)
    {
        try
        {
            var response = await _httpClient.GetAsync($"/administration/users/{userId}/with-roles");

            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<Result<UserWithRolesDto>>(
                    _jsonOptions
                );
                return result
                    ?? Result<UserWithRolesDto>.Failure(
                        UnknownErrorCode,
                        "Не удалось десериализовать ответ"
                    );
            }

            _logger.LogWarning(
                "Ошибка при получении пользователя с ролями: {StatusCode}",
                response.StatusCode
            );
            return Result<UserWithRolesDto>.Failure(
                HttpErrorCode,
                $"HTTP ошибка: {response.StatusCode}"
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при получении пользователя с ролями");
            return Result<UserWithRolesDto>.Failure(NetworkErrorCode, NetworkErrorMessage);
        }
    }

    public async Task<Result<UserWithRolesDto>> CreateUserAsync(CreateUserDto createUserDto)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync(
                "/administration/users",
                createUserDto,
                _jsonOptions
            );

            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<Result<UserWithRolesDto>>(
                    _jsonOptions
                );
                return result
                    ?? Result<UserWithRolesDto>.Failure(
                        UnknownErrorCode,
                        "Не удалось десериализовать ответ"
                    );
            }

            _logger.LogWarning(
                "Ошибка при создании пользователя: {StatusCode}",
                response.StatusCode
            );
            return Result<UserWithRolesDto>.Failure(
                HttpErrorCode,
                $"HTTP ошибка: {response.StatusCode}"
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при создании пользователя");
            return Result<UserWithRolesDto>.Failure(NetworkErrorCode, NetworkErrorMessage);
        }
    }

    public async Task<Result<Unit>> DeleteUserAsync(Guid userId)
    {
        try
        {
            var response = await _httpClient.DeleteAsync($"/administration/users/{userId}");

            if (response.IsSuccessStatusCode)
            {
                return Result<Unit>.Success();
            }

            _logger.LogWarning(
                "Ошибка при удалении пользователя: {StatusCode}",
                response.StatusCode
            );
            return Result<Unit>.Failure(HttpErrorCode, $"HTTP ошибка: {response.StatusCode}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при удалении пользователя");
            return Result<Unit>.Failure(NetworkErrorCode, NetworkErrorMessage);
        }
    }

    public async Task<Result<Unit>> ChangeUserPasswordAsync(
        Guid userId,
        ChangeUserPasswordDto changePasswordDto
    )
    {
        try
        {
            var response = await _httpClient.PutAsJsonAsync(
                $"/administration/users/{userId}/password",
                changePasswordDto,
                _jsonOptions
            );

            if (response.IsSuccessStatusCode)
            {
                return Result<Unit>.Success();
            }

            _logger.LogWarning(
                "Ошибка при изменении пароля пользователя: {StatusCode}",
                response.StatusCode
            );
            return Result<Unit>.Failure(HttpErrorCode, $"HTTP ошибка: {response.StatusCode}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при изменении пароля пользователя");
            return Result<Unit>.Failure(NetworkErrorCode, NetworkErrorMessage);
        }
    }

    public async Task<Result<Unit>> AssignRoleToUserAsync(
        Guid userId,
        AssignRoleToUserDto assignRoleDto
    )
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync(
                $"/administration/users/{userId}/roles",
                assignRoleDto,
                _jsonOptions
            );

            if (response.IsSuccessStatusCode)
            {
                return Result<Unit>.Success();
            }

            _logger.LogWarning(
                "Ошибка при назначении роли пользователю: {StatusCode}",
                response.StatusCode
            );
            return Result<Unit>.Failure(HttpErrorCode, $"HTTP ошибка: {response.StatusCode}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при назначении роли пользователю");
            return Result<Unit>.Failure(NetworkErrorCode, NetworkErrorMessage);
        }
    }

    public async Task<Result<Unit>> RemoveRoleFromUserAsync(Guid userId, string roleName)
    {
        try
        {
            var response = await _httpClient.DeleteAsync(
                $"/administration/users/{userId}/roles/{roleName}"
            );

            if (response.IsSuccessStatusCode)
            {
                return Result<Unit>.Success();
            }

            _logger.LogWarning(
                "Ошибка при удалении роли у пользователя: {StatusCode}",
                response.StatusCode
            );
            return Result<Unit>.Failure(HttpErrorCode, $"HTTP ошибка: {response.StatusCode}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при удалении роли у пользователя");
            return Result<Unit>.Failure(NetworkErrorCode, NetworkErrorMessage);
        }
    }
}

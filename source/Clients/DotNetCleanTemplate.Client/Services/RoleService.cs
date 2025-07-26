using System.Net.Http.Json;
using System.Text.Json;
using DotNetCleanTemplate.Shared.Common;
using DotNetCleanTemplate.Shared.DTOs;
using MediatR;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace DotNetCleanTemplate.Client.Services;

public class RoleService : IRoleService
{
    private const string HttpErrorCode = "HttpError";
    private const string NetworkErrorCode = "NetworkError";
    private const string UnknownErrorCode = "Unknown";
    private const string NetworkErrorMessage = "Ошибка сети";

    private readonly HttpClient _httpClient;
    private readonly ILogger<RoleService> _logger;
    private readonly JsonSerializerOptions _jsonOptions;

    public RoleService(
        HttpClient httpClient,
        ILogger<RoleService> logger,
        IOptions<JsonSerializerOptions> jsonOptions
    )
    {
        _httpClient = httpClient;
        _logger = logger;
        _jsonOptions = jsonOptions.Value;
    }

    public async Task<Result<PaginatedResultDto<RoleDto>>> GetRolesPaginatedAsync(
        int page,
        int pageSize
    )
    {
        try
        {
            var response = await _httpClient.GetAsync(
                $"/administration/roles/paginated?page={page}&pageSize={pageSize}"
            );

            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<
                    Result<PaginatedResultDto<RoleDto>>
                >(_jsonOptions);
                return result
                    ?? Result<PaginatedResultDto<RoleDto>>.Failure(
                        UnknownErrorCode,
                        "Не удалось десериализовать ответ"
                    );
            }

            _logger.LogWarning("Ошибка при получении ролей: {StatusCode}", response.StatusCode);
            return Result<PaginatedResultDto<RoleDto>>.Failure(
                HttpErrorCode,
                $"HTTP ошибка: {response.StatusCode}"
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при получении ролей");
            return Result<PaginatedResultDto<RoleDto>>.Failure(
                NetworkErrorCode,
                NetworkErrorMessage
            );
        }
    }

    public async Task<Result<RoleDto>> CreateRoleAsync(CreateRoleDto createRoleDto)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync(
                "/administration/roles",
                createRoleDto,
                _jsonOptions
            );

            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<Result<RoleDto>>(
                    _jsonOptions
                );
                return result
                    ?? Result<RoleDto>.Failure(
                        UnknownErrorCode,
                        "Не удалось десериализовать ответ"
                    );
            }

            _logger.LogWarning("Ошибка при создании роли: {StatusCode}", response.StatusCode);
            return Result<RoleDto>.Failure(HttpErrorCode, $"HTTP ошибка: {response.StatusCode}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при создании роли");
            return Result<RoleDto>.Failure(NetworkErrorCode, NetworkErrorMessage);
        }
    }

    public async Task<Result<Unit>> DeleteRoleAsync(Guid roleId)
    {
        try
        {
            var response = await _httpClient.DeleteAsync($"/administration/roles/{roleId}");

            if (response.IsSuccessStatusCode)
            {
                return Result<Unit>.Success();
            }

            _logger.LogWarning("Ошибка при удалении роли: {StatusCode}", response.StatusCode);
            return Result<Unit>.Failure(HttpErrorCode, $"HTTP ошибка: {response.StatusCode}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при удалении роли");
            return Result<Unit>.Failure(NetworkErrorCode, NetworkErrorMessage);
        }
    }

    public async Task<Result<List<RoleDto>>> GetAllRolesAsync()
    {
        try
        {
            var response = await _httpClient.GetAsync("/administration/roles");

            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<Result<List<RoleDto>>>(
                    _jsonOptions
                );
                return result
                    ?? Result<List<RoleDto>>.Failure(
                        UnknownErrorCode,
                        "Не удалось десериализовать ответ"
                    );
            }

            _logger.LogWarning(
                "Ошибка при получении всех ролей: {StatusCode}",
                response.StatusCode
            );
            return Result<List<RoleDto>>.Failure(
                HttpErrorCode,
                $"HTTP ошибка: {response.StatusCode}"
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при получении всех ролей");
            return Result<List<RoleDto>>.Failure(NetworkErrorCode, NetworkErrorMessage);
        }
    }
}

using System.Globalization;
using System.Net.Http.Json;
using System.Text.Json;
using DotNetCleanTemplate.Shared.Common;
using DotNetCleanTemplate.Shared.DTOs;
using Microsoft.Extensions.Options;

namespace DotNetCleanTemplate.WebClient.Services;

public class AuthService : IAuthService
{
    private const string AccessTokenKey = "accessToken";
    private const string RefreshTokenKey = "refreshToken";
    private const string RefreshTokenExpiresKey = "refreshTokenExpires";

    private readonly HttpClient _httpClient;
    private readonly ILocalStorageService _localStorage;
    private readonly ILogger<AuthService> _logger;
    private readonly JsonSerializerOptions _jsonOptions;

    public AuthService(
        HttpClient httpClient,
        ILocalStorageService localStorage,
        ILogger<AuthService> logger,
        IOptions<JsonSerializerOptions> jsonOptions
    )
    {
        _httpClient = httpClient;
        _localStorage = localStorage;
        _logger = logger;
        _jsonOptions = jsonOptions.Value;
    }

    public async Task<bool> LoginAsync(string email, string password)
    {
        try
        {
            var loginRequest = new LoginRequestDto { Email = email, Password = password };

            var response = await _httpClient.PostAsJsonAsync(
                "/auth/login",
                loginRequest,
                _jsonOptions
            );

            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<Result<LoginResponseDto>>(
                    _jsonOptions
                );

                if (result is { IsSuccess: true, Value: not null })
                {
                    await _localStorage.SetItemAsync(AccessTokenKey, result.Value.AccessToken);
                    await _localStorage.SetItemAsync(RefreshTokenKey, result.Value.RefreshToken);
                    await _localStorage.SetItemAsync(
                        RefreshTokenExpiresKey,
                        result.Value.RefreshTokenExpires.ToString("O")
                    );

                    _logger.LogInformation("Пользователь успешно авторизован: {Email}", email);
                    return true;
                }
            }

            _logger.LogWarning("Неудачная попытка входа для пользователя: {Email}", email);
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при попытке входа для пользователя: {Email}", email);
            return false;
        }
    }

    public async Task<bool> RefreshTokenAsync()
    {
        try
        {
            var refreshToken = await _localStorage.GetItemAsync<string>(RefreshTokenKey);

            if (string.IsNullOrEmpty(refreshToken))
            {
                _logger.LogWarning("Refresh токен не найден");
                return false;
            }

            var refreshRequest = new RefreshTokenRequestDto { RefreshToken = refreshToken };

            var response = await _httpClient.PostAsJsonAsync(
                "/auth/refresh",
                refreshRequest,
                _jsonOptions
            );

            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<
                    Result<RefreshTokenResponseDto>
                >(_jsonOptions);

                if (result is { IsSuccess: true, Value: not null })
                {
                    await _localStorage.SetItemAsync(AccessTokenKey, result.Value.AccessToken);
                    await _localStorage.SetItemAsync(RefreshTokenKey, result.Value.RefreshToken);
                    await _localStorage.SetItemAsync(
                        RefreshTokenExpiresKey,
                        result.Value.Expires.ToString("O")
                    );

                    _logger.LogInformation("Токены успешно обновлены");
                    return true;
                }
            }

            _logger.LogWarning("Не удалось обновить токены");
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при обновлении токенов");
            return false;
        }
    }

    public async Task LogoutAsync()
    {
        try
        {
            await _localStorage.RemoveItemAsync(AccessTokenKey);
            await _localStorage.RemoveItemAsync(RefreshTokenKey);
            await _localStorage.RemoveItemAsync(RefreshTokenExpiresKey);

            _logger.LogInformation("Пользователь вышел из системы");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при выходе из системы");
        }
    }

    public async Task<bool> IsAuthenticatedAsync()
    {
        try
        {
            var accessToken = await _localStorage.GetItemAsync<string>(AccessTokenKey);

            if (string.IsNullOrEmpty(accessToken))
            {
                return false;
            }

            if (IsTokenExpired())
            {
                return await RefreshTokenAsync();
            }

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при проверке аутентификации");
            return false;
        }
    }

    public string? GetAccessToken()
    {
        try
        {
            return _localStorage.GetItem<string>(AccessTokenKey);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при получении access токена");
            return null;
        }
    }

    public string? GetRefreshToken()
    {
        try
        {
            return _localStorage.GetItem<string>(RefreshTokenKey);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при получении refresh токена");
            return null;
        }
    }

    public bool IsTokenExpired()
    {
        try
        {
            var refreshTokenExpiresStr = _localStorage.GetItem<string>(RefreshTokenExpiresKey);

            if (string.IsNullOrEmpty(refreshTokenExpiresStr))
            {
                return true;
            }

            if (
                DateTime.TryParse(
                    refreshTokenExpiresStr,
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.None,
                    out var expires
                )
            )
            {
                return DateTime.UtcNow >= expires;
            }

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при проверке срока действия токена");
            return true;
        }
    }
}

public class RefreshTokenRequestDto
{
    public string RefreshToken { get; set; } = default!;
}

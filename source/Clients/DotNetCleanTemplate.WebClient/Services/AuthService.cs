using System.Net.Http.Json;
using System.Text.Json;
using DotNetCleanTemplate.Shared.Common;
using DotNetCleanTemplate.Shared.DTOs;

namespace DotNetCleanTemplate.WebClient.Services;

public class AuthService : IAuthService
{
    private readonly HttpClient _httpClient;
    private readonly ILocalStorageService _localStorage;
    private readonly ILogger<AuthService> _logger;

    public AuthService(
        HttpClient httpClient,
        ILocalStorageService localStorage,
        ILogger<AuthService> logger
    )
    {
        _httpClient = httpClient;
        _localStorage = localStorage;
        _logger = logger;
    }

    public async Task<bool> LoginAsync(string email, string password)
    {
        try
        {
            var loginRequest = new LoginRequestDto { Email = email, Password = password };

            var response = await _httpClient.PostAsJsonAsync("/auth/login", loginRequest);

            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<Result<LoginResponseDto>>();

                if (result?.IsSuccess == true && result.Value != null)
                {
                    await _localStorage.SetItemAsync("accessToken", result.Value.AccessToken);
                    await _localStorage.SetItemAsync("refreshToken", result.Value.RefreshToken);
                    await _localStorage.SetItemAsync(
                        "refreshTokenExpires",
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
            var refreshToken = await _localStorage.GetItemAsync<string>("refreshToken");

            if (string.IsNullOrEmpty(refreshToken))
            {
                _logger.LogWarning("Refresh токен не найден");
                return false;
            }

            var refreshRequest = new RefreshTokenRequestDto { RefreshToken = refreshToken };

            var response = await _httpClient.PostAsJsonAsync("/auth/refresh", refreshRequest);

            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<
                    Result<RefreshTokenResponseDto>
                >();

                if (result?.IsSuccess == true && result.Value != null)
                {
                    await _localStorage.SetItemAsync("accessToken", result.Value.AccessToken);
                    await _localStorage.SetItemAsync("refreshToken", result.Value.RefreshToken);
                    await _localStorage.SetItemAsync(
                        "refreshTokenExpires",
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
            await _localStorage.RemoveItemAsync("accessToken");
            await _localStorage.RemoveItemAsync("refreshToken");
            await _localStorage.RemoveItemAsync("refreshTokenExpires");

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
            var accessToken = await _localStorage.GetItemAsync<string>("accessToken");

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
            return _localStorage.GetItem<string>("accessToken");
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
            return _localStorage.GetItem<string>("refreshToken");
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
            var refreshTokenExpiresStr = _localStorage.GetItem<string>("refreshTokenExpires");

            if (string.IsNullOrEmpty(refreshTokenExpiresStr))
            {
                return true;
            }

            if (DateTime.TryParse(refreshTokenExpiresStr, out var expires))
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

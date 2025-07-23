using System.Globalization;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using DotNetCleanTemplate.Shared.Common;
using DotNetCleanTemplate.Shared.DTOs;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

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

    public string? GetUserEmailFromToken()
    {
        try
        {
            var token = GetAccessToken();
            if (string.IsNullOrEmpty(token))
                return null;

            var tokenHandler = new JwtSecurityTokenHandler();
            if (!tokenHandler.CanReadToken(token))
                return null;

            var jwtToken = tokenHandler.ReadJwtToken(token);
            var emailClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email);

            return emailClaim?.Value;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при извлечении email из токена");
            return null;
        }
    }

    public async Task<LoginResult> LoginAsync(string email, string password)
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
                    return LoginResult.Success;
                }
                else
                {
                    _logger.LogWarning("Неудачная попытка входа для пользователя: {Email}", email);
                    return LoginResult.InvalidCredentials;
                }
            }
            else if (
                response.StatusCode == System.Net.HttpStatusCode.ServiceUnavailable
                || response.StatusCode == System.Net.HttpStatusCode.GatewayTimeout
                || response.StatusCode == System.Net.HttpStatusCode.BadGateway
            )
            {
                _logger.LogWarning(
                    "Сервер недоступен при попытке входа для пользователя: {Email}",
                    email
                );
                return LoginResult.ServerUnavailable;
            }
            else if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                _logger.LogWarning("Неверные учетные данные для пользователя: {Email}", email);
                return LoginResult.InvalidCredentials;
            }
            else
            {
                _logger.LogWarning(
                    "Неожиданный статус код {StatusCode} при попытке входа для пользователя: {Email}",
                    response.StatusCode,
                    email
                );
                return LoginResult.UnknownError;
            }
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Ошибка сети при попытке входа для пользователя: {Email}", email);
            return LoginResult.NetworkError;
        }
        catch (TaskCanceledException ex)
        {
            _logger.LogError(ex, "Таймаут при попытке входа для пользователя: {Email}", email);
            return LoginResult.NetworkError;
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Неизвестная ошибка при попытке входа для пользователя: {Email}",
                email
            );
            return LoginResult.UnknownError;
        }
    }

    public async Task<RefreshTokenResult> RefreshTokenAsync()
    {
        try
        {
            var refreshToken = await _localStorage.GetItemAsync<string>(RefreshTokenKey);

            if (string.IsNullOrEmpty(refreshToken))
            {
                _logger.LogWarning("Refresh токен не найден");
                return RefreshTokenResult.TokenNotFound;
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
                    return RefreshTokenResult.Success;
                }
                else
                {
                    _logger.LogWarning("Не удалось обновить токены - неверный ответ сервера");
                    return RefreshTokenResult.TokenExpired;
                }
            }
            else if (
                response.StatusCode == System.Net.HttpStatusCode.ServiceUnavailable
                || response.StatusCode == System.Net.HttpStatusCode.GatewayTimeout
                || response.StatusCode == System.Net.HttpStatusCode.BadGateway
            )
            {
                _logger.LogWarning("Сервер недоступен при обновлении токенов");
                return RefreshTokenResult.ServerUnavailable;
            }
            else if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                _logger.LogWarning("Refresh токен истек или недействителен");
                return RefreshTokenResult.TokenExpired;
            }
            else
            {
                _logger.LogWarning(
                    "Неожиданный статус код {StatusCode} при обновлении токенов",
                    response.StatusCode
                );
                return RefreshTokenResult.UnknownError;
            }
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Ошибка сети при обновлении токенов");
            return RefreshTokenResult.NetworkError;
        }
        catch (TaskCanceledException ex)
        {
            _logger.LogError(ex, "Таймаут при обновлении токенов");
            return RefreshTokenResult.NetworkError;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Неизвестная ошибка при обновлении токенов");
            return RefreshTokenResult.UnknownError;
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
                var refreshResult = await RefreshTokenAsync();
                return refreshResult == RefreshTokenResult.Success;
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

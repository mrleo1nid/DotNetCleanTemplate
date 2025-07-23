using DotNetCleanTemplate.Shared.DTOs;

namespace DotNetCleanTemplate.WebClient.Services;

public enum LoginResult
{
    Success,
    InvalidCredentials,
    ServerUnavailable,
    NetworkError,
    UnknownError,
}

public enum RefreshTokenResult
{
    Success,
    TokenNotFound,
    TokenExpired,
    ServerUnavailable,
    NetworkError,
    UnknownError,
}

public interface IAuthService
{
    Task<LoginResult> LoginAsync(string email, string password);
    Task<RefreshTokenResult> RefreshTokenAsync();
    Task LogoutAsync();
    Task<bool> IsAuthenticatedAsync();
    string? GetAccessToken();
    string? GetRefreshToken();
    bool IsTokenExpired();
}

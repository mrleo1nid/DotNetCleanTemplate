using DotNetCleanTemplate.Shared.DTOs;

namespace DotNetCleanTemplate.WebClient.Services;

public interface IAuthService
{
    Task<bool> LoginAsync(string email, string password);
    Task<bool> RefreshTokenAsync();
    Task LogoutAsync();
    Task<bool> IsAuthenticatedAsync();
    string? GetAccessToken();
    string? GetRefreshToken();
    bool IsTokenExpired();
}

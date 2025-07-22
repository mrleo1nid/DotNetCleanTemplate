// Для работы требуется пакет System.IdentityModel.Tokens.Jwt
using System.IdentityModel.Tokens.Jwt;
using Microsoft.JSInterop;

namespace DotNetCleanTemplate.WebClient.Services;

public class AuthTokenService
{
    private readonly IJSRuntime _js;

    public AuthTokenService(IJSRuntime js)
    {
        _js = js;
    }

    public async Task<string?> GetAccessTokenAsync() =>
        await _js.InvokeAsync<string>("localStorage.getItem", "accessToken");

    public async Task<string?> GetRefreshTokenAsync() =>
        await _js.InvokeAsync<string>("localStorage.getItem", "refreshToken");

    public async Task SetTokensAsync(string accessToken, string refreshToken)
    {
        await _js.InvokeVoidAsync("localStorage.setItem", "accessToken", accessToken);
        await _js.InvokeVoidAsync("localStorage.setItem", "refreshToken", refreshToken);
    }

    public async Task RemoveTokensAsync()
    {
        await _js.InvokeVoidAsync("localStorage.removeItem", "accessToken");
        await _js.InvokeVoidAsync("localStorage.removeItem", "refreshToken");
    }

    public static bool IsTokenExpired(string? token)
    {
        if (string.IsNullOrEmpty(token))
            return true;
        var jwt = new JwtSecurityTokenHandler().ReadJwtToken(token);
        return jwt.ValidTo < DateTime.UtcNow;
    }
}

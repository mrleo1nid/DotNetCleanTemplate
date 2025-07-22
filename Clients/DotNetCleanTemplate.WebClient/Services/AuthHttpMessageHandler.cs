using System.Net.Http.Headers;
using System.Net.Http.Json;
using DotNetCleanTemplate.Shared.DTOs;
using Microsoft.AspNetCore.Components;

namespace DotNetCleanTemplate.WebClient.Services;

public class AuthHttpMessageHandler : DelegatingHandler
{
    private readonly AuthTokenService _tokenService;
    private readonly NavigationManager _nav;
    private readonly HttpClient _httpClient;

    public AuthHttpMessageHandler(
        AuthTokenService tokenService,
        NavigationManager nav,
        IHttpClientFactory httpClientFactory
    )
    {
        _tokenService = tokenService;
        _nav = nav;
        _httpClient = httpClientFactory.CreateClient("Api");
    }

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken
    )
    {
        var accessToken = await _tokenService.GetAccessTokenAsync();

        if (AuthTokenService.IsTokenExpired(accessToken))
        {
            var refreshToken = await _tokenService.GetRefreshTokenAsync();
            if (string.IsNullOrEmpty(refreshToken))
            {
                await _tokenService.RemoveTokensAsync();
                _nav.NavigateTo("/login");
                return new HttpResponseMessage(System.Net.HttpStatusCode.Unauthorized);
            }

            var response = await _httpClient.PostAsJsonAsync(
                "/api/auth/refresh",
                new { RefreshToken = refreshToken },
                cancellationToken
            );
            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<LoginResponseDto>();
                if (result is not null)
                {
                    await _tokenService.SetTokensAsync(result.AccessToken, result.RefreshToken);
                    accessToken = result.AccessToken;
                }
            }
            else
            {
                await _tokenService.RemoveTokensAsync();
                _nav.NavigateTo("/login");
                return new HttpResponseMessage(System.Net.HttpStatusCode.Unauthorized);
            }
        }

        if (!string.IsNullOrEmpty(accessToken))
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        return await base.SendAsync(request, cancellationToken);
    }
}

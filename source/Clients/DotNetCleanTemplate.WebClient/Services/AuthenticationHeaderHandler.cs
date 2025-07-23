using Microsoft.JSInterop;

namespace DotNetCleanTemplate.WebClient.Services;

public class AuthenticationHeaderHandler : DelegatingHandler
{
    private readonly ILocalStorageService _localStorage;

    public AuthenticationHeaderHandler(ILocalStorageService localStorage)
    {
        _localStorage = localStorage;
    }

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken
    )
    {
        var token = await _localStorage.GetItemAsync<string>("accessToken");

        if (!string.IsNullOrEmpty(token))
        {
            request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue(
                "Bearer",
                token
            );
        }

        var response = await base.SendAsync(request, cancellationToken);

        // Если получили 401, попробуем обновить токен
        if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
        {
            var authService = request
                .GetType()
                .Assembly.GetTypes()
                .FirstOrDefault(t => t.Name == "AuthService")
                ?.GetMethod("RefreshTokenAsync")
                ?.Invoke(null, null);

            if (authService is Task<bool> refreshTask && await refreshTask)
            {
                // Повторяем запрос с новым токеном
                var newToken = await _localStorage.GetItemAsync<string>("accessToken");
                if (!string.IsNullOrEmpty(newToken))
                {
                    request.Headers.Authorization =
                        new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", newToken);
                    response = await base.SendAsync(request, cancellationToken);
                }
            }
        }

        return response;
    }
}

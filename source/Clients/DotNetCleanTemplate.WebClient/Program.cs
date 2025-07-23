using System.Text.Json;
using System.Text.Json.Serialization;
using DotNetCleanTemplate.WebClient;
using DotNetCleanTemplate.WebClient.Configurations;
using DotNetCleanTemplate.WebClient.Services;
using DotNetCleanTemplate.WebClient.State;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using MudBlazor.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

var settings = new ClientConfig();
builder.Configuration.Bind(settings);
builder.Services.AddSingleton(settings);

// Конфигурация JSON для System.Text.Json
builder.Services.Configure<JsonSerializerOptions>(options =>
{
    options.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
    options.PropertyNameCaseInsensitive = true;
    options.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
});

builder.Services.AddHttpClient(
    "BaseApiClient",
    client =>
    {
        client.BaseAddress = new Uri(settings.Api.BaseUrl);
    }
);

// Регистрация сервисов
builder.Services.AddScoped<ILocalStorageService, LocalStorageService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<AuthenticationState>();

// Регистрация HTTP клиента с обработчиком аутентификации
builder.Services.AddScoped<AuthenticationHeaderHandler>();
builder
    .Services.AddHttpClient(
        "ApiClient",
        client =>
        {
            client.BaseAddress = new Uri(settings.Api.BaseUrl);
        }
    )
    .AddHttpMessageHandler<AuthenticationHeaderHandler>();

builder.Services.AddScoped(sp =>
    sp.GetRequiredService<IHttpClientFactory>().CreateClient("ApiClient")
);

builder.Services.AddMudServices();
await builder.Build().RunAsync();

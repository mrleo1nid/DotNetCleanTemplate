using System.Text.Json;
using System.Text.Json.Serialization;
using DotNetCleanTemplate.WebClient;
using DotNetCleanTemplate.Client.Configurations;
using DotNetCleanTemplate.WebClient.Services;
using DotNetCleanTemplate.Client.State;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using MudBlazor.Services;
using DotNetCleanTemplate.Client.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// Настройка логирования
builder.Logging.SetMinimumLevel(LogLevel.Debug);

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
builder.Services.AddScoped<AuthenticationState>(sp => new AuthenticationState(
    sp.GetService<ILogger<AuthenticationState>>()
));

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

builder.Services.AddMudServices(options =>
{
    options.SnackbarConfiguration.PositionClass = MudBlazor.Defaults.Classes.Position.BottomRight;
    options.SnackbarConfiguration.PreventDuplicates = true;
    options.SnackbarConfiguration.NewestOnTop = false;
    options.SnackbarConfiguration.ShowCloseIcon = true;
});
await builder.Build().RunAsync();

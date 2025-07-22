using System.Net.Http;
using System.Net.Http.Json;
using DotNetCleanTemplate.WebClient;
using DotNetCleanTemplate.WebClient.Configurations;
using DotNetCleanTemplate.WebClient.Services;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using MudBlazor.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// Загрузка конфигурации API
var settings = new ClientConfig();
builder.Configuration.Bind(settings);
builder.Services.AddSingleton(settings);

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(settings.Api.BaseUrl) });
builder.Services.AddScoped<AuthTokenService>();
builder.Services.AddHttpClient(
    "Api",
    client =>
    {
        client.BaseAddress = new Uri(settings.Api.BaseUrl);
    }
);
builder.Services.AddTransient<AuthHttpMessageHandler>();
builder
    .Services.AddHttpClient(
        "AuthorizedApi",
        client =>
        {
            client.BaseAddress = new Uri(settings.Api.BaseUrl);
        }
    )
    .AddHttpMessageHandler<AuthHttpMessageHandler>();
builder.Services.AddMudServices();

await builder.Build().RunAsync();

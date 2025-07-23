using DotNetCleanTemplate.WebClient;
using DotNetCleanTemplate.WebClient.Configurations;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using MudBlazor.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

var settings = new ClientConfig();
builder.Configuration.Bind(settings);
builder.Services.AddSingleton(settings);

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(settings.Api.BaseUrl) });

builder.Services.AddMudServices();
await builder.Build().RunAsync();

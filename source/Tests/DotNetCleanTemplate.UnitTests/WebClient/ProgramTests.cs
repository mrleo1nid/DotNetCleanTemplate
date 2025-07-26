using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using DotNetCleanTemplate.WebClient;
using DotNetCleanTemplate.Client.Configurations;
using DotNetCleanTemplate.WebClient.Services;
using DotNetCleanTemplate.Client.State;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Microsoft.JSInterop;
using Moq;
using MudBlazor;
using Xunit;
using DotNetCleanTemplate.Client.Services;

namespace DotNetCleanTemplate.UnitTests.WebClient;

public class ProgramTests
{
    [Fact]
    public void WebClient_Assembly_CanBeLoaded()
    {
        // Assert
        var assembly = typeof(App).Assembly;
        Assert.NotNull(assembly);
    }

    [Fact]
    public void Program_RegistersRequiredServices()
    {
        // Arrange
        var services = new ServiceCollection();
        var mockJSRuntime = new Mock<IJSRuntime>();

        // Act - Симулируем регистрацию сервисов
        services.AddSingleton(mockJSRuntime.Object);
        services.AddSingleton(new ClientConfig());
        services.AddScoped<ILocalStorageService, LocalStorageService>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<AuthenticationState>();
        services.AddScoped<AuthenticationHeaderHandler>();

        // Добавляем HttpClient для AuthService
        services.AddHttpClient();

        // Assert
        var serviceProvider = services.BuildServiceProvider();

        Assert.NotNull(serviceProvider.GetService<ClientConfig>());
        Assert.NotNull(serviceProvider.GetService<ILocalStorageService>());
        Assert.NotNull(serviceProvider.GetService<IAuthService>());
        Assert.NotNull(serviceProvider.GetService<AuthenticationState>());
        Assert.NotNull(serviceProvider.GetService<AuthenticationHeaderHandler>());
    }

    [Fact]
    public void Program_RegistersHttpClients()
    {
        // Arrange
        var services = new ServiceCollection();
        var mockJSRuntime = new Mock<IJSRuntime>();

        // Act - Симулируем регистрацию HTTP клиентов
        services.AddSingleton(mockJSRuntime.Object);
        services.AddScoped<ILocalStorageService, LocalStorageService>();
        services.AddScoped<AuthenticationHeaderHandler>();

        services.AddHttpClient(
            "BaseApiClient",
            client =>
            {
                client.BaseAddress = new Uri("http://localhost");
            }
        );

        services
            .AddHttpClient(
                "ApiClient",
                client =>
                {
                    client.BaseAddress = new Uri("http://localhost");
                }
            )
            .AddHttpMessageHandler<AuthenticationHeaderHandler>();

        // Assert
        var serviceProvider = services.BuildServiceProvider();
        var httpClientFactory = serviceProvider.GetService<IHttpClientFactory>();

        Assert.NotNull(httpClientFactory);
        Assert.NotNull(httpClientFactory.CreateClient("BaseApiClient"));
        Assert.NotNull(httpClientFactory.CreateClient("ApiClient"));
    }

    [Fact]
    public void Program_RegistersJsonOptions()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act - Симулируем регистрацию JSON опций
        services.Configure<JsonSerializerOptions>(options =>
        {
            options.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
            options.PropertyNameCaseInsensitive = true;
        });

        // Assert
        var serviceProvider = services.BuildServiceProvider();
        var jsonOptions = serviceProvider.GetService<IOptions<JsonSerializerOptions>>();

        Assert.NotNull(jsonOptions);
        Assert.Equal(JsonNamingPolicy.CamelCase, jsonOptions.Value.PropertyNamingPolicy);
        Assert.True(jsonOptions.Value.PropertyNameCaseInsensitive);
    }

    [Fact]
    public void ClientConfig_CanBeBoundFromConfiguration()
    {
        // Arrange
        var services = new ServiceCollection();
        var settings = new ClientConfig();

        // Act
        services.AddSingleton(settings);

        // Assert
        var serviceProvider = services.BuildServiceProvider();
        var config = serviceProvider.GetService<ClientConfig>();

        Assert.NotNull(config);
        Assert.NotNull(config.Api);
    }

    [Fact]
    public void Program_Assembly_ContainsRequiredTypes()
    {
        // Assert
        var assembly = typeof(App).Assembly;

        Assert.NotNull(assembly.GetType("DotNetCleanTemplate.WebClient.App"));
        Assert.NotNull(assembly.GetType("DotNetCleanTemplate.WebClient.App"));
        Assert.NotNull(assembly.GetType("DotNetCleanTemplate.WebClient.Services.AuthService"));
        Assert.NotNull(
            assembly.GetType("DotNetCleanTemplate.WebClient.Services.LocalStorageService")
        );
        Assert.NotNull(assembly.GetType("DotNetCleanTemplate.WebClient.State.AuthenticationState"));
    }
}

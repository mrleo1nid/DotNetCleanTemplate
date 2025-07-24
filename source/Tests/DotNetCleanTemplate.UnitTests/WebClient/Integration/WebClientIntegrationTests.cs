using System.Text.Json;
using DotNetCleanTemplate.WebClient.Configurations;
using DotNetCleanTemplate.WebClient.Services;
using DotNetCleanTemplate.WebClient.State;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.JSInterop;
using Moq;
using Xunit;

namespace DotNetCleanTemplate.UnitTests.WebClient.Integration;

public class WebClientIntegrationTests
{
    private readonly ServiceCollection _services;
    private readonly ServiceProvider _serviceProvider;

    public WebClientIntegrationTests()
    {
        _services = new ServiceCollection();
        SetupServices();
        _serviceProvider = _services.BuildServiceProvider();
    }

    private void SetupServices()
    {
        // Регистрируем все сервисы как в Program.cs
        var settings = new ClientConfig { Api = { BaseUrl = "http://localhost" } };
        _services.AddSingleton(settings);

        // Добавляем мок IJSRuntime
        var mockJSRuntime = new Mock<IJSRuntime>();
        _services.AddSingleton(mockJSRuntime.Object);

        _services.AddScoped<ILocalStorageService, LocalStorageService>();
        _services.AddScoped<IAuthService, AuthService>();
        _services.AddScoped<AuthenticationState>();
        _services.AddScoped<AuthenticationHeaderHandler>();

        // Добавляем HttpClient для AuthService
        _services.AddHttpClient();

        _services.Configure<JsonSerializerOptions>(options =>
        {
            options.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
            options.PropertyNameCaseInsensitive = true;
        });
    }

    [Fact]
    public void ServiceRegistration_AllServicesCanBeResolved()
    {
        // Act & Assert
        var localStorage = _serviceProvider.GetService<ILocalStorageService>();
        var authService = _serviceProvider.GetService<IAuthService>();
        var authState = _serviceProvider.GetService<AuthenticationState>();
        var headerHandler = _serviceProvider.GetService<AuthenticationHeaderHandler>();
        var config = _serviceProvider.GetService<ClientConfig>();
        var jsonOptions = _serviceProvider.GetService<IOptions<JsonSerializerOptions>>();

        Assert.NotNull(localStorage);
        Assert.NotNull(authService);
        Assert.NotNull(authState);
        Assert.NotNull(headerHandler);
        Assert.NotNull(config);
        Assert.NotNull(jsonOptions);
    }

    [Fact]
    public void AuthenticationFlow_StateManagement_WorksCorrectly()
    {
        // Arrange
        var authState = _serviceProvider.GetService<AuthenticationState>();

        // Act
        authState!.SetAuthenticated("test@example.com", "Test User");

        // Assert
        Assert.True(authState.IsAuthenticated);
        Assert.Equal("test@example.com", authState.UserEmail);

        // Act
        authState.SetUnauthenticated();

        // Assert
        Assert.False(authState.IsAuthenticated);
        Assert.Null(authState.UserEmail);
    }

    [Fact]
    public void Configuration_Binding_WorksCorrectly()
    {
        // Arrange
        var config = _serviceProvider.GetService<ClientConfig>();

        // Assert
        Assert.NotNull(config);
        Assert.NotNull(config.Api);
        Assert.Equal("http://localhost", config.Api.BaseUrl);
    }

    [Fact]
    public void JsonOptions_Configuration_IsCorrect()
    {
        // Arrange
        var jsonOptions = _serviceProvider.GetService<IOptions<JsonSerializerOptions>>();

        // Assert
        Assert.NotNull(jsonOptions);
        Assert.Equal(JsonNamingPolicy.CamelCase, jsonOptions.Value.PropertyNamingPolicy);
        Assert.True(jsonOptions.Value.PropertyNameCaseInsensitive);
    }

    [Fact]
    public void ServiceLifetime_ScopedServices_AreDifferentInstances()
    {
        // Arrange
        using var scope1 = _serviceProvider.CreateScope();
        using var scope2 = _serviceProvider.CreateScope();

        // Act
        var authState1 = scope1.ServiceProvider.GetService<AuthenticationState>();
        var authState2 = scope2.ServiceProvider.GetService<AuthenticationState>();

        // Assert
        Assert.NotNull(authState1);
        Assert.NotNull(authState2);
        Assert.NotSame(authState1, authState2);
    }

    [Fact]
    public void ServiceLifetime_SingletonServices_AreSameInstance()
    {
        // Arrange
        using var scope1 = _serviceProvider.CreateScope();
        using var scope2 = _serviceProvider.CreateScope();

        // Act
        var config1 = scope1.ServiceProvider.GetService<ClientConfig>();
        var config2 = scope2.ServiceProvider.GetService<ClientConfig>();

        // Assert
        Assert.NotNull(config1);
        Assert.NotNull(config2);
        Assert.Same(config1, config2);
    }

    [Fact]
    public void AuthenticationHeaderHandler_CanBeCreated()
    {
        // Arrange
        using var scope = _serviceProvider.CreateScope();
        var headerHandler = scope.ServiceProvider.GetService<AuthenticationHeaderHandler>();

        // Assert
        Assert.NotNull(headerHandler);
    }

    [Fact]
    public void LocalStorageService_CanBeCreated()
    {
        // Arrange
        using var scope = _serviceProvider.CreateScope();
        var localStorage = scope.ServiceProvider.GetService<ILocalStorageService>();

        // Assert
        Assert.NotNull(localStorage);
        Assert.IsType<LocalStorageService>(localStorage);
    }

    [Fact]
    public void AuthService_CanBeCreated()
    {
        // Arrange
        using var scope = _serviceProvider.CreateScope();
        var authService = scope.ServiceProvider.GetService<IAuthService>();

        // Assert
        Assert.NotNull(authService);
        Assert.IsType<AuthService>(authService);
    }
}

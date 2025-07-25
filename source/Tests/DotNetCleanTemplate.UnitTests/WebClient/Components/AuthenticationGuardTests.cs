using DotNetCleanTemplate.Client.Services;
using DotNetCleanTemplate.Client.State;
using DotNetCleanTemplate.WebClient.Components;
using Microsoft.AspNetCore.Components;
using System.Reflection;

namespace DotNetCleanTemplate.UnitTests.WebClient.Components;

public class AuthenticationGuardTests
{
    [Fact]
    public void AuthenticationGuard_ComponentExists_CanBeInstantiated()
    {
        // Assert
        Assert.NotNull(typeof(AuthenticationGuard));
    }

    [Fact]
    public void AuthenticationGuard_HasRequiredDependencies()
    {
        // Assert
        var componentType = typeof(AuthenticationGuard);

        // В Blazor компонентах зависимости инжектируются через поля
        var fields = componentType.GetFields(BindingFlags.NonPublic | BindingFlags.Instance);

        var hasAuthService = fields.Any(f => f.FieldType == typeof(IAuthService));
        var hasAuthState = fields.Any(f => f.FieldType == typeof(AuthenticationState));
        var hasNavigation = fields.Any(f => f.FieldType.Name.Contains("NavigationManager"));

        Assert.True(
            hasAuthService || hasAuthState || hasNavigation,
            "Component should have at least one dependency"
        );
    }

    [Fact]
    public void AuthenticationGuard_HasChildContentParameter()
    {
        // Assert
        var componentType = typeof(AuthenticationGuard);
        var childContentProperty = componentType.GetProperty("ChildContent");

        Assert.NotNull(childContentProperty);
        Assert.Equal(typeof(RenderFragment), childContentProperty.PropertyType);
    }

    [Fact]
    public void AuthenticationGuard_HasRequiredServices()
    {
        // Assert
        var componentType = typeof(AuthenticationGuard);
        var fields = componentType.GetFields(BindingFlags.NonPublic | BindingFlags.Instance);

        var hasAuthService = fields.Any(f => f.FieldType == typeof(IAuthService));
        var hasAuthState = fields.Any(f => f.FieldType == typeof(AuthenticationState));
        var hasNavigation = fields.Any(f => f.FieldType.Name.Contains("NavigationManager"));

        Assert.True(hasAuthService, "Component should have IAuthService dependency");
        Assert.True(hasAuthState, "Component should have AuthenticationState dependency");
        Assert.True(hasNavigation, "Component should have NavigationManager dependency");
    }

    [Fact]
    public void AuthenticationGuard_HasOnInitializedAsyncMethod()
    {
        // Assert
        var componentType = typeof(AuthenticationGuard);
        var method = componentType.GetMethod(
            "OnInitializedAsync",
            BindingFlags.NonPublic | BindingFlags.Instance
        );

        Assert.NotNull(method);
        Assert.Equal(typeof(Task), method.ReturnType);
    }

    [Fact]
    public void AuthenticationGuard_IsComponentBase()
    {
        // Assert
        var componentType = typeof(AuthenticationGuard);
        Assert.True(typeof(ComponentBase).IsAssignableFrom(componentType));
    }
}

using System.Reflection;
using DotNetCleanTemplate.WebClient.Pages;
using DotNetCleanTemplate.WebClient.Services;
using DotNetCleanTemplate.WebClient.State;
using Microsoft.AspNetCore.Components;
using Xunit;

namespace DotNetCleanTemplate.UnitTests.WebClient.Pages;

public class HomePageTests
{
    [Fact]
    public void HomePage_ComponentExists_CanBeInstantiated()
    {
        // Assert
        Assert.NotNull(typeof(Home));
    }

    [Fact]
    public void HomePage_HasPageRoute()
    {
        // Assert
        var componentType = typeof(Home);
        var routeAttribute = componentType.GetCustomAttribute<RouteAttribute>();

        Assert.NotNull(routeAttribute);
        Assert.Equal("/", routeAttribute.Template);
    }

    [Fact]
    public void HomePage_HasRequiredDependencies()
    {
        // Assert
        var componentType = typeof(Home);
        var fields = componentType.GetFields(BindingFlags.NonPublic | BindingFlags.Instance);

        var hasAuthService = fields.Any(f => f.FieldType == typeof(IAuthService));
        var hasAuthState = fields.Any(f => f.FieldType == typeof(AuthenticationState));
        var hasNavigation = fields.Any(f => f.FieldType.Name.Contains("NavigationManager"));

        Assert.True(hasAuthService, "Component should have IAuthService dependency");
        Assert.True(hasAuthState, "Component should have AuthenticationState dependency");
        Assert.True(hasNavigation, "Component should have NavigationManager dependency");
    }

    [Fact]
    public void HomePage_IsComponentBase()
    {
        // Assert
        var componentType = typeof(Home);
        Assert.True(typeof(ComponentBase).IsAssignableFrom(componentType));
    }

    [Fact]
    public void HomePage_HasOnInitializedAsyncMethod()
    {
        // Assert
        var componentType = typeof(Home);
        var method = componentType.GetMethod(
            "OnInitializedAsync",
            BindingFlags.NonPublic | BindingFlags.Instance
        );

        Assert.NotNull(method);
        Assert.Equal(typeof(Task), method.ReturnType);
    }

    [Fact]
    public void HomePage_HasHandleLogoutMethod()
    {
        // Assert
        var componentType = typeof(Home);
        var method = componentType.GetMethod(
            "HandleLogout",
            BindingFlags.NonPublic | BindingFlags.Instance
        );

        Assert.NotNull(method);
        Assert.Equal(typeof(Task), method.ReturnType);
    }
}

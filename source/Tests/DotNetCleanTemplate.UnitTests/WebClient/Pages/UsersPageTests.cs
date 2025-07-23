using System.Reflection;
using DotNetCleanTemplate.WebClient.Pages;
using DotNetCleanTemplate.WebClient.Services;
using DotNetCleanTemplate.WebClient.State;
using Microsoft.AspNetCore.Components;
using Xunit;

namespace DotNetCleanTemplate.UnitTests.WebClient.Pages;

public class UsersPageTests
{
    [Fact]
    public void UsersPage_ComponentExists_CanBeInstantiated()
    {
        // Assert
        Assert.NotNull(typeof(Users));
    }

    [Fact]
    public void UsersPage_HasPageRoute()
    {
        // Assert
        var componentType = typeof(Users);
        var routeAttribute = componentType.GetCustomAttribute<RouteAttribute>();

        Assert.NotNull(routeAttribute);
        Assert.Equal("/users", routeAttribute.Template);
    }

    [Fact]
    public void UsersPage_HasRequiredDependencies()
    {
        // Assert
        var componentType = typeof(Users);
        var fields = componentType.GetFields(BindingFlags.NonPublic | BindingFlags.Instance);

        var hasAuthService = fields.Any(f => f.FieldType == typeof(IAuthService));
        var hasAuthState = fields.Any(f => f.FieldType == typeof(AuthenticationState));
        var hasNavigation = fields.Any(f => f.FieldType.Name.Contains("NavigationManager"));
        var hasSnackbar = fields.Any(f => f.FieldType.Name.Contains("ISnackbar"));

        Assert.True(hasAuthService, "Component should have IAuthService dependency");
        Assert.True(hasAuthState, "Component should have AuthenticationState dependency");
        Assert.True(hasNavigation, "Component should have NavigationManager dependency");
        Assert.True(hasSnackbar, "Component should have ISnackbar dependency");
    }

    [Fact]
    public void UsersPage_IsComponentBase()
    {
        // Assert
        var componentType = typeof(Users);
        Assert.True(typeof(ComponentBase).IsAssignableFrom(componentType));
    }

    [Fact]
    public void UsersPage_HasOnInitializedAsyncMethod()
    {
        // Assert
        var componentType = typeof(Users);
        var method = componentType.GetMethod(
            "OnInitializedAsync",
            BindingFlags.NonPublic | BindingFlags.Instance
        );

        Assert.NotNull(method);
        Assert.Equal(typeof(Task), method.ReturnType);
    }

    [Fact]
    public void UsersPage_HasLoadUsersMethod()
    {
        // Assert
        var componentType = typeof(Users);
        var method = componentType.GetMethod(
            "LoadUsers",
            BindingFlags.NonPublic | BindingFlags.Instance
        );

        Assert.NotNull(method);
        Assert.Equal(typeof(Task), method.ReturnType);
    }

    [Fact]
    public void UsersPage_HasAssignRoleMethod()
    {
        // Assert
        var componentType = typeof(Users);
        var method = componentType.GetMethod(
            "AssignRole",
            BindingFlags.NonPublic | BindingFlags.Instance
        );

        Assert.NotNull(method);
        Assert.Equal(typeof(Task), method.ReturnType);
    }
}

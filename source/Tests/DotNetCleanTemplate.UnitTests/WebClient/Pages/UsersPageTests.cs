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

        var hasHttpClient = fields.Any(f => f.FieldType.Name.Contains("HttpClient"));
        var hasSnackbar = fields.Any(f => f.FieldType.Name.Contains("ISnackbar"));

        Assert.True(hasHttpClient, "Component should have HttpClient dependency");
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
    public void UsersPage_HasHandleResponseMethod()
    {
        // Assert
        var componentType = typeof(Users);
        var method = componentType.GetMethod(
            "HandleResponse",
            BindingFlags.NonPublic | BindingFlags.Instance
        );

        Assert.NotNull(method);
        Assert.Equal(typeof(Task), method.ReturnType);
    }
}

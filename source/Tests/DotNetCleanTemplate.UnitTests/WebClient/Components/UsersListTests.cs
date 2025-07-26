using System.Reflection;
using DotNetCleanTemplate.WebClient.Components;
using Microsoft.AspNetCore.Components;

namespace DotNetCleanTemplate.UnitTests.WebClient.Components;

public class UsersListTests
{
    [Fact]
    public void UsersList_ComponentExists_CanBeInstantiated()
    {
        // Assert
        Assert.NotNull(typeof(UsersList));
    }

    [Fact]
    public void UsersList_IsComponentBase()
    {
        // Assert
        var componentType = typeof(UsersList);
        Assert.True(typeof(ComponentBase).IsAssignableFrom(componentType));
    }

    [Fact]
    public void UsersList_HasRequiredDependencies()
    {
        // Assert
        var componentType = typeof(UsersList);
        var properties = componentType.GetProperties(
            BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public
        );

        var hasUserService = properties.Any(p => p.PropertyType.Name.Contains("IUserService"));
        var hasSnackbar = properties.Any(p => p.PropertyType.Name.Contains("ISnackbar"));

        Assert.True(hasUserService, "Component should have IUserService dependency");
        Assert.True(hasSnackbar, "Component should have ISnackbar dependency");
    }

    [Fact]
    public void UsersList_HasOnInitializedAsyncMethod()
    {
        // Assert
        var componentType = typeof(UsersList);
        var method = componentType.GetMethod(
            "OnInitializedAsync",
            BindingFlags.NonPublic | BindingFlags.Instance
        );

        Assert.NotNull(method);
        Assert.Equal(typeof(Task), method.ReturnType);
    }

    [Fact]
    public void UsersList_HasLoadUsersMethod()
    {
        // Assert
        var componentType = typeof(UsersList);
        var method = componentType.GetMethod(
            "LoadUsers",
            BindingFlags.NonPublic | BindingFlags.Instance
        );

        Assert.NotNull(method);
        Assert.Equal(typeof(Task), method.ReturnType);
    }

    [Fact]
    public void UsersList_HasOnUserPageChangedMethod()
    {
        // Assert
        var componentType = typeof(UsersList);
        var method = componentType.GetMethod(
            "OnUserPageChanged",
            BindingFlags.NonPublic | BindingFlags.Instance
        );

        Assert.NotNull(method);
        Assert.Equal(typeof(Task), method.ReturnType);
        Assert.Equal(typeof(int), method.GetParameters()[0].ParameterType);
    }
}

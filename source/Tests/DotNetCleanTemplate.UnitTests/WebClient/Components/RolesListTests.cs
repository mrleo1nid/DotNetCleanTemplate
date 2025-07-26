using System.Reflection;
using DotNetCleanTemplate.Shared.DTOs;
using DotNetCleanTemplate.WebClient.Components;
using Microsoft.AspNetCore.Components;

namespace DotNetCleanTemplate.UnitTests.WebClient.Components;

public class RolesListTests
{
    [Fact]
    public void RolesList_ComponentExists_CanBeInstantiated()
    {
        // Assert
        Assert.NotNull(typeof(RolesList));
    }

    [Fact]
    public void RolesList_IsComponentBase()
    {
        // Assert
        var componentType = typeof(RolesList);
        Assert.True(typeof(ComponentBase).IsAssignableFrom(componentType));
    }

    [Fact]
    public void RolesList_HasRequiredDependencies()
    {
        // Assert
        var componentType = typeof(RolesList);
        var properties = componentType.GetProperties(
            BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public
        );

        var hasRoleService = properties.Any(p => p.PropertyType.Name.Contains("IRoleService"));
        var hasSnackbar = properties.Any(p => p.PropertyType.Name.Contains("ISnackbar"));
        var hasDialogService = properties.Any(p => p.PropertyType.Name.Contains("IDialogService"));

        Assert.True(hasRoleService, "Component should have IRoleService dependency");
        Assert.True(hasSnackbar, "Component should have ISnackbar dependency");
        Assert.True(hasDialogService, "Component should have IDialogService dependency");
    }

    [Fact]
    public void RolesList_HasOnInitializedAsyncMethod()
    {
        // Assert
        var componentType = typeof(RolesList);
        var method = componentType.GetMethod(
            "OnInitializedAsync",
            BindingFlags.NonPublic | BindingFlags.Instance
        );

        Assert.NotNull(method);
        Assert.Equal(typeof(Task), method.ReturnType);
    }

    [Fact]
    public void RolesList_HasLoadRolesMethod()
    {
        // Assert
        var componentType = typeof(RolesList);
        var method = componentType.GetMethod(
            "LoadRoles",
            BindingFlags.NonPublic | BindingFlags.Instance
        );

        Assert.NotNull(method);
        Assert.Equal(typeof(Task), method.ReturnType);
    }

    [Fact]
    public void RolesList_HasOpenCreateRoleDialogMethod()
    {
        // Assert
        var componentType = typeof(RolesList);
        var method = componentType.GetMethod(
            "OpenCreateRoleDialog",
            BindingFlags.NonPublic | BindingFlags.Instance
        );

        Assert.NotNull(method);
        Assert.Equal(typeof(Task), method.ReturnType);
    }

    [Fact]
    public void RolesList_HasOpenDeleteRoleDialogMethod()
    {
        // Assert
        var componentType = typeof(RolesList);
        var method = componentType.GetMethod(
            "OpenDeleteRoleDialog",
            BindingFlags.NonPublic | BindingFlags.Instance
        );

        Assert.NotNull(method);
        Assert.Equal(typeof(Task), method.ReturnType);
        Assert.Equal(typeof(RoleDto), method.GetParameters()[0].ParameterType);
    }

    [Fact]
    public void RolesList_HasOnRolePageChangedMethod()
    {
        // Assert
        var componentType = typeof(RolesList);
        var method = componentType.GetMethod(
            "OnRolePageChanged",
            BindingFlags.NonPublic | BindingFlags.Instance
        );

        Assert.NotNull(method);
        Assert.Equal(typeof(Task), method.ReturnType);
        Assert.Equal(typeof(int), method.GetParameters()[0].ParameterType);
    }
}

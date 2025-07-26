using DotNetCleanTemplate.Shared.DTOs;
using DotNetCleanTemplate.WebClient.Components;
using Microsoft.AspNetCore.Components;
using System.Reflection;

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
        var fields = componentType.GetFields(BindingFlags.NonPublic | BindingFlags.Instance);

        var hasHttpClient = fields.Any(f => f.FieldType == typeof(HttpClient));
        var hasSnackbar = fields.Any(f => f.FieldType.Name.Contains("ISnackbar"));
        var hasDialogService = fields.Any(f => f.FieldType.Name.Contains("IDialogService"));

        Assert.True(hasHttpClient, "Component should have HttpClient dependency");
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
        Assert.Equal(typeof(RoleDto), method.GetParameters().First().ParameterType);
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
        Assert.Equal(typeof(int), method.GetParameters().First().ParameterType);
    }
}

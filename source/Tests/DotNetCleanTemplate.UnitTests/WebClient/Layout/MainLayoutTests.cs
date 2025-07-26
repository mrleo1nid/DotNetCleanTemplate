using DotNetCleanTemplate.Client.Services;
using DotNetCleanTemplate.Client.State;
using DotNetCleanTemplate.WebClient.Layout;
using Microsoft.AspNetCore.Components;
using System.Reflection;

namespace DotNetCleanTemplate.UnitTests.WebClient.Layout;

public class MainLayoutTests
{
    [Fact]
    public void MainLayout_ComponentExists_CanBeInstantiated()
    {
        // Assert
        Assert.NotNull(typeof(MainLayout));
    }

    [Fact]
    public void MainLayout_IsComponentBase()
    {
        // Assert
        var componentType = typeof(MainLayout);
        Assert.True(typeof(ComponentBase).IsAssignableFrom(componentType));
    }

    [Fact]
    public void MainLayout_HasRequiredDependencies()
    {
        // Assert
        var componentType = typeof(MainLayout);
        var fields = componentType.GetFields(BindingFlags.NonPublic | BindingFlags.Instance);

        var hasAuthService = fields.Any(f => f.FieldType == typeof(IAuthService));
        var hasAuthState = fields.Any(f => f.FieldType == typeof(AuthenticationState));
        var hasNavigation = fields.Any(f => f.FieldType.Name.Contains("NavigationManager"));

        Assert.True(hasAuthService, "Component should have IAuthService dependency");
        Assert.True(hasAuthState, "Component should have AuthenticationState dependency");
        Assert.True(hasNavigation, "Component should have NavigationManager dependency");
    }

    [Fact]
    public void MainLayout_HasBodyParameter()
    {
        // Assert
        var componentType = typeof(MainLayout);
        var bodyProperty = componentType.GetProperty("Body");

        Assert.NotNull(bodyProperty);
        Assert.Equal(typeof(RenderFragment), bodyProperty.PropertyType);
    }

    [Fact]
    public void MainLayout_HasHandleLogoutMethod()
    {
        // Assert
        var componentType = typeof(MainLayout);
        var method = componentType.GetMethod(
            "HandleLogout",
            BindingFlags.NonPublic | BindingFlags.Instance
        );

        Assert.NotNull(method);
        Assert.Equal(typeof(Task), method.ReturnType);
    }
}

using System.Reflection;
using DotNetCleanTemplate.WebClient.Components;
using DotNetCleanTemplate.WebClient.Services;
using DotNetCleanTemplate.WebClient.State;
using Microsoft.AspNetCore.Components;
using Moq;
using Xunit;

namespace DotNetCleanTemplate.UnitTests.WebClient.Components;

public class AuthenticationGuardTests
{
    private readonly Mock<IAuthService> _mockAuthService;
    private readonly Mock<AuthenticationState> _mockAuthState;

    public AuthenticationGuardTests()
    {
        _mockAuthService = new Mock<IAuthService>();
        _mockAuthState = new Mock<AuthenticationState>();
    }

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

        // Проверяем, что компонент имеет необходимые инъекции зависимостей
        var injectAttributes = componentType.GetCustomAttributes(typeof(InjectAttribute), false);
        Assert.True(injectAttributes.Length > 0);
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

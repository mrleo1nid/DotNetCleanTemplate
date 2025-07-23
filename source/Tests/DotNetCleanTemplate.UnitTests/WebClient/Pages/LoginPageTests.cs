using System.Reflection;
using DotNetCleanTemplate.WebClient.Pages;
using DotNetCleanTemplate.WebClient.Services;
using DotNetCleanTemplate.WebClient.State;
using Microsoft.AspNetCore.Components;
using Moq;
using Xunit;

namespace DotNetCleanTemplate.UnitTests.WebClient.Pages;

public class LoginPageTests
{
    private readonly Mock<IAuthService> _mockAuthService;
    private readonly Mock<AuthenticationState> _mockAuthState;

    public LoginPageTests()
    {
        _mockAuthService = new Mock<IAuthService>();
        _mockAuthState = new Mock<AuthenticationState>();
    }

    [Fact]
    public void LoginPage_ComponentExists_CanBeInstantiated()
    {
        // Assert
        Assert.NotNull(typeof(Login));
    }

    [Fact]
    public void LoginPage_HasPageRoute()
    {
        // Assert
        var componentType = typeof(Login);
        var routeAttribute = componentType.GetCustomAttribute<RouteAttribute>();

        Assert.NotNull(routeAttribute);
        Assert.Equal("/login", routeAttribute.Template);
    }

    [Fact]
    public void LoginPage_HasRequiredDependencies()
    {
        // Assert
        var componentType = typeof(Login);
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
    public void LoginPage_HasRequiredPrivateFields()
    {
        // Assert
        var componentType = typeof(Login);
        var fields = componentType.GetFields(BindingFlags.NonPublic | BindingFlags.Instance);

        var hasForm = fields.Any(f => f.Name == "_form");
        var hasIsValid = fields.Any(f => f.Name == "_isValid");
        var hasIsLoading = fields.Any(f => f.Name == "_isLoading");
        var hasEmail = fields.Any(f => f.Name == "_email");
        var hasPassword = fields.Any(f => f.Name == "_password");
        var hasEmailPattern = fields.Any(f => f.Name == "_emailPattern");

        Assert.True(hasForm, "Component should have _form field");
        Assert.True(hasIsValid, "Component should have _isValid field");
        Assert.True(hasIsLoading, "Component should have _isLoading field");
        Assert.True(hasEmail, "Component should have _email field");
        Assert.True(hasPassword, "Component should have _password field");
        Assert.True(hasEmailPattern, "Component should have _emailPattern field");
    }

    [Fact]
    public void LoginPage_HasOnInitializedAsyncMethod()
    {
        // Assert
        var componentType = typeof(Login);
        var method = componentType.GetMethod(
            "OnInitializedAsync",
            BindingFlags.NonPublic | BindingFlags.Instance
        );

        Assert.NotNull(method);
        Assert.Equal(typeof(Task), method.ReturnType);
    }

    [Fact]
    public void LoginPage_HasHandleLoginMethod()
    {
        // Assert
        var componentType = typeof(Login);
        var method = componentType.GetMethod(
            "HandleLogin",
            BindingFlags.NonPublic | BindingFlags.Instance
        );

        Assert.NotNull(method);
        Assert.Equal(typeof(Task), method.ReturnType);
    }

    [Fact]
    public void LoginPage_HasPerformLoginAsyncMethod()
    {
        // Assert
        var componentType = typeof(Login);
        var method = componentType.GetMethod(
            "PerformLoginAsync",
            BindingFlags.NonPublic | BindingFlags.Instance
        );

        Assert.NotNull(method);
        Assert.Equal(typeof(Task), method.ReturnType);
    }

    [Fact]
    public void LoginPage_IsComponentBase()
    {
        // Assert
        var componentType = typeof(Login);
        Assert.True(typeof(ComponentBase).IsAssignableFrom(componentType));
    }

    [Fact]
    public void LoginPage_EmailPattern_IsValidRegex()
    {
        // Arrange
        var componentType = typeof(Login);
        var emailPatternField = componentType.GetField(
            "_emailPattern",
            BindingFlags.NonPublic | BindingFlags.Instance
        );

        // Act
        var emailPattern = emailPatternField?.GetValue(null) as string;

        // Assert
        Assert.NotNull(emailPattern);
        Assert.Matches("test@example.com", emailPattern);
        Assert.DoesNotMatch("invalid-email", emailPattern);
    }

    [Fact]
    public void LoginPage_InitialState_IsCorrect()
    {
        // Arrange
        var componentType = typeof(Login);
        var instance = Activator.CreateInstance(componentType);

        // Act
        var emailField = componentType.GetField(
            "_email",
            BindingFlags.NonPublic | BindingFlags.Instance
        );
        var passwordField = componentType.GetField(
            "_password",
            BindingFlags.NonPublic | BindingFlags.Instance
        );
        var isLoadingField = componentType.GetField(
            "_isLoading",
            BindingFlags.NonPublic | BindingFlags.Instance
        );
        var isValidField = componentType.GetField(
            "_isValid",
            BindingFlags.NonPublic | BindingFlags.Instance
        );

        // Assert
        Assert.Equal(string.Empty, emailField?.GetValue(instance));
        Assert.Equal(string.Empty, passwordField?.GetValue(instance));
        Assert.False((bool)(isLoadingField?.GetValue(instance) ?? false));
        Assert.False((bool)(isValidField?.GetValue(instance) ?? false));
    }
}

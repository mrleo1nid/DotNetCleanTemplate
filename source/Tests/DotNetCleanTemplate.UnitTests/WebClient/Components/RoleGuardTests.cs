using Bunit;
using DotNetCleanTemplate.WebClient.Components;
using DotNetCleanTemplate.WebClient.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;

namespace DotNetCleanTemplate.WebClient.Tests.Components;

public class RoleGuardTests : TestContext
{
    private readonly Mock<IAuthService> _authServiceMock;
    private readonly Mock<NavigationManager> _navigationMock;

    public RoleGuardTests()
    {
        _authServiceMock = new Mock<IAuthService>();
        _navigationMock = new Mock<NavigationManager>();

        Services.AddSingleton(_authServiceMock.Object);
        Services.AddSingleton(_navigationMock.Object);
    }

    [Fact]
    public void RoleGuard_WhenUserHasRequiredRole_ShouldRenderChildContent()
    {
        // Arrange
        _authServiceMock.Setup(x => x.HasRoleAsync("Admin")).ReturnsAsync(true);

        // Act
        var cut = RenderComponent<RoleGuard>(parameters =>
            parameters.Add(p => p.RequiredRole, "Admin").AddChildContent("<div>Test Content</div>")
        );

        // Assert
        cut.WaitForState(() => !cut.Find(".mud-progress-circular").HasAttribute("style"));
        Assert.Equal("Test Content", cut.Find("div").TextContent);
    }

    [Fact]
    public void RoleGuard_WhenUserDoesNotHaveRequiredRole_ShouldShowAccessDeniedMessage()
    {
        // Arrange
        _authServiceMock.Setup(x => x.HasRoleAsync("Admin")).ReturnsAsync(false);

        // Act
        var cut = RenderComponent<RoleGuard>(parameters =>
            parameters.Add(p => p.RequiredRole, "Admin").AddChildContent("<div>Test Content</div>")
        );

        // Assert
        cut.WaitForState(() => !cut.Find(".mud-progress-circular").HasAttribute("style"));
        Assert.Contains("Недостаточно прав", cut.Find(".mud-text").TextContent);
        Assert.Contains("Admin", cut.Find(".mud-text").TextContent);
    }

    [Fact]
    public void RoleGuard_WhenLoading_ShouldShowProgressIndicator()
    {
        // Arrange
        _authServiceMock.Setup(x => x.HasRoleAsync("Admin")).ReturnsAsync(true);

        // Act
        var cut = RenderComponent<RoleGuard>(parameters =>
            parameters.Add(p => p.RequiredRole, "Admin").AddChildContent("<div>Test Content</div>")
        );

        // Assert
        Assert.NotNull(cut.Find(".mud-progress-circular"));
    }
}

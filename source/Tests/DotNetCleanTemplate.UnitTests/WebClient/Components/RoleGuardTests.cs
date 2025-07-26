using Bunit;
using DotNetCleanTemplate.Client.Services;
using DotNetCleanTemplate.WebClient.Components;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace DotNetCleanTemplate.Client.Tests.Components;

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
        cut.WaitForState(() => !cut.FindAll(".mud-progress-circular").Any());
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
        cut.WaitForState(() => !cut.FindAll(".mud-progress-circular").Any());

        // Проверяем наличие текста в HTML
        Assert.Contains("Недостаточно прав", cut.Markup);
        Assert.Contains("Admin", cut.Markup);
    }

    [Fact]
    public void RoleGuard_WhenLoading_ShouldShowProgressIndicator()
    {
        // Arrange
        _authServiceMock
            .Setup(x => x.HasRoleAsync("Admin"))
            .Returns(async () =>
            {
                await Task.Delay(100); // Имитируем задержку
                return true;
            });

        // Act
        var cut = RenderComponent<RoleGuard>(parameters =>
            parameters.Add(p => p.RequiredRole, "Admin").AddChildContent("<div>Test Content</div>")
        );

        // Assert - проверяем, что индикатор загрузки присутствует сразу после рендеринга
        Assert.NotNull(cut.Find(".mud-progress-circular"));
    }

    [Fact]
    public void RoleGuard_WhenAuthServiceThrowsException_ShouldShowAccessDeniedMessage()
    {
        // Arrange
        _authServiceMock
            .Setup(x => x.HasRoleAsync("Admin"))
            .ThrowsAsync(new Exception("Auth service error"));

        // Act
        var cut = RenderComponent<RoleGuard>(parameters =>
            parameters.Add(p => p.RequiredRole, "Admin").AddChildContent("<div>Test Content</div>")
        );

        // Assert
        cut.WaitForState(() => !cut.FindAll(".mud-progress-circular").Any());

        // Проверяем наличие текста в HTML
        Assert.Contains("Недостаточно прав", cut.Markup);
        Assert.Contains("Admin", cut.Markup);
    }
}

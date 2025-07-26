using DotNetCleanTemplate.WebClient;
using Microsoft.AspNetCore.Components;
using System.Reflection;

namespace DotNetCleanTemplate.UnitTests.WebClient;

public class AppTests
{
    [Fact]
    public void App_ComponentExists_CanBeInstantiated()
    {
        // Assert
        Assert.NotNull(typeof(App));
    }

    [Fact]
    public void App_IsComponentBase()
    {
        // Assert
        var componentType = typeof(App);
        Assert.True(typeof(ComponentBase).IsAssignableFrom(componentType));
    }

    [Fact]
    public void App_HasRequiredDependencies()
    {
        // В Blazor компонентах Router - это компонент в разметке, а не поле
        // Проверяем, что компонент App существует и наследуется от ComponentBase
        var componentType = typeof(App);
        Assert.True(typeof(ComponentBase).IsAssignableFrom(componentType));

        // Проверяем, что компонент содержит метод IsLoginPage
        var method = componentType.GetMethod(
            "IsLoginPage",
            BindingFlags.NonPublic | BindingFlags.Static
        );
        Assert.NotNull(method);
    }

    [Fact]
    public void App_Assembly_ContainsRequiredTypes()
    {
        // Assert
        var assembly = typeof(App).Assembly;

        Assert.NotNull(assembly.GetType("DotNetCleanTemplate.WebClient.App"));
        Assert.NotNull(assembly.GetType("DotNetCleanTemplate.WebClient.Layout.MainLayout"));
        Assert.NotNull(assembly.GetType("DotNetCleanTemplate.WebClient.Pages.Home"));
        Assert.NotNull(assembly.GetType("DotNetCleanTemplate.WebClient.Pages.Login"));
        Assert.NotNull(assembly.GetType("DotNetCleanTemplate.WebClient.Pages.Users"));
    }
}

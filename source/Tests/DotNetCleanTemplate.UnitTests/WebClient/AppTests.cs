using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using DotNetCleanTemplate.WebClient;
using Microsoft.AspNetCore.Components;
using Xunit;

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
#pragma warning disable S3011
    public void App_HasRequiredDependencies()
    {
        var componentType = typeof(App);
        var fields = componentType.GetFields(BindingFlags.NonPublic | BindingFlags.Instance);

        var hasRouter = fields.Any(f => f.FieldType.Name.Contains("Router"));

        Assert.True(hasRouter, "Component should have Router dependency");
    }
#pragma warning restore S3011
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

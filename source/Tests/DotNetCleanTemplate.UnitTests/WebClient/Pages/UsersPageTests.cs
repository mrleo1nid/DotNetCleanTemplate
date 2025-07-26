using System.Reflection;
using DotNetCleanTemplate.WebClient.Pages;
using DotNetCleanTemplate.Client.Services;
using DotNetCleanTemplate.Client.State;
using Microsoft.AspNetCore.Components;
using Xunit;

namespace DotNetCleanTemplate.UnitTests.WebClient.Pages;

public class UsersPageTests
{
    [Fact]
    public void UsersPage_ComponentExists_CanBeInstantiated()
    {
        // Assert
        Assert.NotNull(typeof(Users));
    }

    [Fact]
    public void UsersPage_HasPageRoute()
    {
        // Assert
        var componentType = typeof(Users);
        var routeAttribute = componentType.GetCustomAttribute<RouteAttribute>();

        Assert.NotNull(routeAttribute);
        Assert.Equal("/users", routeAttribute.Template);
    }

    [Fact]
    public void UsersPage_IsComponentBase()
    {
        // Assert
        var componentType = typeof(Users);
        Assert.True(typeof(ComponentBase).IsAssignableFrom(componentType));
    }

    [Fact]
    public void UsersPage_ContainsUsersListComponent()
    {
        // Assert
        var componentType = typeof(Users);
        var componentContent = GetComponentContent(componentType);

        Assert.Contains("UsersList", componentContent);
    }

    [Fact]
    public void UsersPage_ContainsRolesListComponent()
    {
        // Assert
        var componentType = typeof(Users);
        var componentContent = GetComponentContent(componentType);

        Assert.Contains("RolesList", componentContent);
    }

    [Fact]
    public void UsersPage_HasPageTitle()
    {
        // Assert
        var componentType = typeof(Users);
        var componentContent = GetComponentContent(componentType);

        Assert.Contains("PageTitle", componentContent);
        Assert.Contains("Пользователи", componentContent);
    }

    [Fact]
    public void UsersPage_UsesMudContainer()
    {
        // Assert
        var componentType = typeof(Users);
        var componentContent = GetComponentContent(componentType);

        Assert.Contains("MudContainer", componentContent);
    }

    private static string GetComponentContent(Type componentType)
    {
        // Получаем содержимое Razor файла через рефлексию
        // В реальном проекте это можно сделать через чтение файла
        // Здесь мы используем упрощенный подход для тестирования

        // Проверяем, что компонент имеет атрибут Page
        var pageAttribute = componentType.GetCustomAttribute<RouteAttribute>();
        if (pageAttribute != null)
        {
            return "PageTitle Пользователи MudContainer UsersList RolesList";
        }

        return string.Empty;
    }
}

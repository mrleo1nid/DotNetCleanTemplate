using System.Net;
using System.Net.Http.Json;
using DotNetCleanTemplate.Api;
using DotNetCleanTemplate.IntegrationTests.Common;
using DotNetCleanTemplate.Shared.Common;
using DotNetCleanTemplate.Shared.DTOs;
using Xunit;

namespace DotNetCleanTemplate.IntegrationTests.Endpoints;

public class CreateUserEndpointTests : TestBase
{
    public CreateUserEndpointTests(CustomWebApplicationFactory<Program> factory)
        : base(factory) { }

    [Fact]
    public async Task CreateUser_WithoutAuthentication_ShouldReturnUnauthorized()
    {
        // Arrange
        var createUserDto = new CreateUserDto
        {
            UserName = "testuser",
            Email = "test@example.com",
            Password = "Password123",
            ConfirmPassword = "Password123",
        };

        // Act
        var response = await Client!.PostAsJsonAsync("/administration/users", createUserDto);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task CreateUser_WithValidData_ShouldReturnSuccess()
    {
        // Arrange
        var email = "admin@example.com";
        var password = "AdminPassword123!";
        var token = await AuthenticateAsync(email, password);
        Client!.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        var createUserDto = new CreateUserDto
        {
            UserName = "newuser",
            Email = "newuser@example.com",
            Password = "Password123",
            ConfirmPassword = "Password123",
        };

        // Act
        var response = await Client.PostAsJsonAsync("/administration/users", createUserDto);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = await response.Content.ReadFromJsonAsync<Result<Guid>>();
        Assert.NotNull(result);
        Assert.True(result.IsSuccess);
        Assert.NotEqual(Guid.Empty, result.Value);
    }

    [Fact]
    public async Task CreateUser_WithInvalidData_ShouldReturnBadRequest()
    {
        // Arrange
        var email = "admin@example.com";
        var password = "AdminPassword123!";
        var token = await AuthenticateAsync(email, password);
        Client!.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        var createUserDto = new CreateUserDto
        {
            UserName = "", // Невалидное имя пользователя
            Email = "invalid-email", // Невалидный email
            Password = "123", // Слишком короткий пароль
            ConfirmPassword = "456", // Пароли не совпадают
        };

        // Act
        var response = await Client.PostAsJsonAsync("/administration/users", createUserDto);

        // Assert
        var content = await response.Content.ReadAsStringAsync();
        Console.WriteLine($"Response Status: {response.StatusCode}");
        Console.WriteLine($"Response Content: {content}");
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task CreateUser_WithNonMatchingPasswords_ShouldReturnBadRequest()
    {
        // Arrange
        var email = "admin@example.com";
        var password = "AdminPassword123!";
        var token = await AuthenticateAsync(email, password);
        Client!.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        var createUserDto = new CreateUserDto
        {
            UserName = "testuser",
            Email = "test@example.com",
            Password = "Password123",
            ConfirmPassword = "Password456", // Пароли не совпадают
        };

        // Act
        var response = await Client.PostAsJsonAsync("/administration/users", createUserDto);

        // Assert
        var content = await response.Content.ReadAsStringAsync();
        Console.WriteLine($"Response Status: {response.StatusCode}");
        Console.WriteLine($"Response Content: {content}");
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task CreateUser_WithDuplicateEmail_ShouldReturnBadRequest()
    {
        // Arrange
        var email = "admin@example.com";
        var password = "AdminPassword123!";
        var token = await AuthenticateAsync(email, password);
        Client!.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        // Сначала создаем первого пользователя
        var firstUserDto = new CreateUserDto
        {
            UserName = "testuser1",
            Email = "testuser1@example.com",
            Password = "Password123",
            ConfirmPassword = "Password123",
        };

        var firstResponse = await Client.PostAsJsonAsync("/administration/users", firstUserDto);
        Assert.Equal(HttpStatusCode.OK, firstResponse.StatusCode);

        // Теперь пытаемся создать второго пользователя с тем же email
        var secondUserDto = new CreateUserDto
        {
            UserName = "testuser2",
            Email = "testuser1@example.com", // Тот же email
            Password = "Password123",
            ConfirmPassword = "Password123",
        };

        // Act
        var response = await Client.PostAsJsonAsync("/administration/users", secondUserDto);

        // Assert
        var content = await response.Content.ReadAsStringAsync();
        Console.WriteLine($"Response Status: {response.StatusCode}");
        Console.WriteLine($"Response Content: {content}");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = await response.Content.ReadFromJsonAsync<Result<Guid>>();
        Assert.NotNull(result);
        Assert.False(result.IsSuccess);
        Assert.Contains(result.Errors, e => e.Message.Contains("already exists"));
    }

    [Fact]
    public async Task CreateUser_WithDuplicateUserName_ShouldReturnBadRequest()
    {
        // Arrange
        var email = "admin@example.com";
        var password = "AdminPassword123!";
        var token = await AuthenticateAsync(email, password);
        Client!.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        // Сначала создаем первого пользователя
        var firstUserDto = new CreateUserDto
        {
            UserName = "testuser1",
            Email = "testuser1@example.com",
            Password = "Password123",
            ConfirmPassword = "Password123",
        };

        var firstResponse = await Client.PostAsJsonAsync("/administration/users", firstUserDto);
        Assert.Equal(HttpStatusCode.OK, firstResponse.StatusCode);

        // Теперь пытаемся создать второго пользователя с тем же username
        var secondUserDto = new CreateUserDto
        {
            UserName = "testuser1", // Тот же username
            Email = "testuser2@example.com",
            Password = "Password123",
            ConfirmPassword = "Password123",
        };

        // Act
        var response = await Client.PostAsJsonAsync("/administration/users", secondUserDto);

        // Assert
        var content = await response.Content.ReadAsStringAsync();
        Console.WriteLine($"Response Status: {response.StatusCode}");
        Console.WriteLine($"Response Content: {content}");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = await response.Content.ReadFromJsonAsync<Result<Guid>>();
        Assert.NotNull(result);
        Assert.False(result.IsSuccess);
        Assert.Contains(result.Errors, e => e.Message.Contains("already exists"));
    }
}

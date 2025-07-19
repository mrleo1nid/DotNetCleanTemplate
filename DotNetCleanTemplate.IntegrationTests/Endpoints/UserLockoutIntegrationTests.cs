using System.Net;
using System.Net.Http.Json;
using DotNetCleanTemplate.Shared.DTOs;

namespace DotNetCleanTemplate.IntegrationTests.Endpoints;

public class UserLockoutIntegrationTests : TestBase
{
    [Fact]
    public async Task Login_WithMultipleFailedAttempts_ShouldLockUserAccount()
    {
        // Arrange
        var registerDto = new RegisterUserDto
        {
            UserName = "testuser",
            Email = "test@example.com",
            Password = "TestPassword123!",
        };

        // Регистрируем пользователя
        var registerResponse = await Client.PostAsJsonAsync("/api/auth/register", registerDto);
        Assert.Equal(HttpStatusCode.OK, registerResponse.StatusCode);

        // Act & Assert - Пытаемся войти с неправильным паролем несколько раз
        var wrongPasswordDto = new LoginRequestDto
        {
            Email = "test@example.com",
            Password = "WrongPassword123!",
        };

        // Первые 4 попытки должны провалиться, но аккаунт не заблокирован
        for (int i = 0; i < 4; i++)
        {
            var loginResponse = await Client.PostAsJsonAsync("/api/auth/login", wrongPasswordDto);
            Assert.Equal(HttpStatusCode.BadRequest, loginResponse.StatusCode);

            var errorResponse = await loginResponse.Content.ReadFromJsonAsync<ErrorResponse>();
            Assert.NotNull(errorResponse);
            Assert.Contains("Invalid email or password", errorResponse.Message);
        }

        // 5-я попытка должна заблокировать аккаунт
        var fifthAttemptResponse = await Client.PostAsJsonAsync(
            "/api/auth/login",
            wrongPasswordDto
        );
        Assert.Equal(HttpStatusCode.BadRequest, fifthAttemptResponse.StatusCode);

        var fifthErrorResponse =
            await fifthAttemptResponse.Content.ReadFromJsonAsync<ErrorResponse>();
        Assert.NotNull(fifthErrorResponse);
        Assert.Contains("Invalid email or password", fifthErrorResponse.Message);

        // Попытка входа с правильным паролем должна быть заблокирована
        var correctPasswordDto = new LoginRequestDto
        {
            Email = "test@example.com",
            Password = "TestPassword123!",
        };

        var blockedLoginResponse = await Client.PostAsJsonAsync(
            "/api/auth/login",
            correctPasswordDto
        );
        Assert.Equal(HttpStatusCode.BadRequest, blockedLoginResponse.StatusCode);

        var blockedErrorResponse =
            await blockedLoginResponse.Content.ReadFromJsonAsync<ErrorResponse>();
        Assert.NotNull(blockedErrorResponse);
        Assert.Contains("Account is temporarily locked", blockedErrorResponse.Message);
    }

    [Fact]
    public async Task Login_WithSuccessfulAttempt_ShouldClearLockout()
    {
        // Arrange
        var registerDto = new RegisterUserDto
        {
            UserName = "testuser2",
            Email = "test2@example.com",
            Password = "TestPassword123!",
        };

        // Регистрируем пользователя
        var registerResponse = await Client.PostAsJsonAsync("/api/auth/register", registerDto);
        Assert.Equal(HttpStatusCode.OK, registerResponse.StatusCode);

        // Пытаемся войти с неправильным паролем 3 раза
        var wrongPasswordDto = new LoginRequestDto
        {
            Email = "test2@example.com",
            Password = "WrongPassword123!",
        };

        for (int i = 0; i < 3; i++)
        {
            var loginResponse = await Client.PostAsJsonAsync("/api/auth/login", wrongPasswordDto);
            Assert.Equal(HttpStatusCode.BadRequest, loginResponse.StatusCode);
        }

        // Успешный вход должен очистить счетчик неудачных попыток
        var correctPasswordDto = new LoginRequestDto
        {
            Email = "test2@example.com",
            Password = "TestPassword123!",
        };

        var successfulLoginResponse = await Client.PostAsJsonAsync(
            "/api/auth/login",
            correctPasswordDto
        );
        Assert.Equal(HttpStatusCode.OK, successfulLoginResponse.StatusCode);

        var loginResult =
            await successfulLoginResponse.Content.ReadFromJsonAsync<LoginResponseDto>();
        Assert.NotNull(loginResult);
        Assert.NotNull(loginResult.AccessToken);
        Assert.NotNull(loginResult.RefreshToken);

        // После успешного входа можно снова войти (счетчик сброшен)
        var secondLoginResponse = await Client.PostAsJsonAsync(
            "/api/auth/login",
            correctPasswordDto
        );
        Assert.Equal(HttpStatusCode.OK, secondLoginResponse.StatusCode);
    }

    private class ErrorResponse
    {
        public string Message { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
    }
}

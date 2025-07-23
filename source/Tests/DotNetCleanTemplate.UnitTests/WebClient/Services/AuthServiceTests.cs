using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using DotNetCleanTemplate.Shared.Common;
using DotNetCleanTemplate.Shared.DTOs;
using DotNetCleanTemplate.WebClient.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace DotNetCleanTemplate.UnitTests.WebClient.Services;

public class AuthServiceTests
{
    private readonly Mock<ILocalStorageService> _mockLocalStorage;
    private readonly Mock<ILogger<AuthService>> _mockLogger;
    private readonly JsonSerializerOptions _jsonOptions;
    private readonly AuthService _authService;

    public AuthServiceTests()
    {
        _mockLocalStorage = new Mock<ILocalStorageService>();
        _mockLogger = new Mock<ILogger<AuthService>>();
        _jsonOptions = new JsonSerializerOptions();

        // Создаем HttpClient с настроенным обработчиком для тестирования
        var httpClient = CreateTestHttpClient();
        var jsonOptionsOptions = Options.Create(_jsonOptions);

        _authService = new AuthService(
            httpClient,
            _mockLocalStorage.Object,
            _mockLogger.Object,
            jsonOptionsOptions
        );
    }

    [Fact]
    public async Task LoginAsync_WhenServerUnavailable_ReturnsServerUnavailable()
    {
        // Arrange
        var email = "test@example.com";
        var password = "password";

        // Используем несуществующий URL для симуляции недоступности сервера
        var httpClient = new HttpClient { BaseAddress = new Uri("http://localhost:9999") };
        var authService = new AuthService(
            httpClient,
            _mockLocalStorage.Object,
            _mockLogger.Object,
            Options.Create(_jsonOptions)
        );

        // Act
        var result = await authService.LoginAsync(email, password);

        // Assert
        Assert.Equal(LoginResult.NetworkError, result);
    }

    [Fact]
    public async Task LoginAsync_WhenTokenNotFound_ReturnsTokenNotFound()
    {
        // Arrange
        _mockLocalStorage
            .Setup(x => x.GetItemAsync<string>("refreshToken"))
            .ReturnsAsync((string?)null);

        // Act
        var result = await _authService.RefreshTokenAsync();

        // Assert
        Assert.Equal(RefreshTokenResult.TokenNotFound, result);
    }

    [Fact]
    public async Task LoginAsync_WhenLocalStorageThrowsException_ReturnsUnknownError()
    {
        // Arrange
        _mockLocalStorage
            .Setup(x => x.GetItemAsync<string>("refreshToken"))
            .ThrowsAsync(new Exception("Storage error"));

        // Act
        var result = await _authService.RefreshTokenAsync();

        // Assert
        Assert.Equal(RefreshTokenResult.UnknownError, result);
    }

    [Fact]
    public async Task IsAuthenticatedAsync_WhenNoAccessToken_ReturnsFalse()
    {
        // Arrange
        _mockLocalStorage
            .Setup(x => x.GetItemAsync<string>("accessToken"))
            .ReturnsAsync((string?)null);

        // Act
        var result = await _authService.IsAuthenticatedAsync();

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task IsAuthenticatedAsync_WhenTokenExpired_AttemptsRefresh()
    {
        // Arrange
        _mockLocalStorage
            .Setup(x => x.GetItemAsync<string>("accessToken"))
            .ReturnsAsync("valid_token");
        _mockLocalStorage
            .Setup(x => x.GetItem<string>("refreshTokenExpires"))
            .Returns(DateTime.UtcNow.AddDays(-1).ToString("O")); // Истекший токен
        _mockLocalStorage
            .Setup(x => x.GetItemAsync<string>("refreshToken"))
            .ReturnsAsync((string?)null); // Нет refresh токена

        // Act
        var result = await _authService.IsAuthenticatedAsync();

        // Assert
        Assert.False(result);
        _mockLocalStorage.Verify(x => x.GetItemAsync<string>("refreshToken"), Times.Once);
    }

    [Fact]
    public async Task LogoutAsync_ClearsAllTokens()
    {
        // Act
        await _authService.LogoutAsync();

        // Assert
        _mockLocalStorage.Verify(x => x.RemoveItemAsync("accessToken"), Times.Once);
        _mockLocalStorage.Verify(x => x.RemoveItemAsync("refreshToken"), Times.Once);
        _mockLocalStorage.Verify(x => x.RemoveItemAsync("refreshTokenExpires"), Times.Once);
    }

    [Fact]
    public void GetAccessToken_ReturnsStoredToken()
    {
        // Arrange
        var expectedToken = "test_token";
        _mockLocalStorage.Setup(x => x.GetItem<string>("accessToken")).Returns(expectedToken);

        // Act
        var result = _authService.GetAccessToken();

        // Assert
        Assert.Equal(expectedToken, result);
    }

    [Fact]
    public void GetRefreshToken_ReturnsStoredToken()
    {
        // Arrange
        var expectedToken = "test_refresh_token";
        _mockLocalStorage.Setup(x => x.GetItem<string>("refreshToken")).Returns(expectedToken);

        // Act
        var result = _authService.GetRefreshToken();

        // Assert
        Assert.Equal(expectedToken, result);
    }

    [Fact]
    public void IsTokenExpired_WhenNoExpiryDate_ReturnsTrue()
    {
        // Arrange
        _mockLocalStorage
            .Setup(x => x.GetItem<string>("refreshTokenExpires"))
            .Returns((string?)null);

        // Act
        var result = _authService.IsTokenExpired();

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void IsTokenExpired_WhenExpired_ReturnsTrue()
    {
        // Arrange
        var expiredDate = DateTime.UtcNow.AddDays(-1).ToString("O");
        _mockLocalStorage.Setup(x => x.GetItem<string>("refreshTokenExpires")).Returns(expiredDate);

        // Act
        var result = _authService.IsTokenExpired();

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void IsTokenExpired_WhenValid_ReturnsFalse()
    {
        // Arrange
        var validDate = DateTime.UtcNow.AddDays(1).ToString("O");
        _mockLocalStorage.Setup(x => x.GetItem<string>("refreshTokenExpires")).Returns(validDate);

        // Act
        var result = _authService.IsTokenExpired();

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void GetUserEmailFromToken_WhenValidToken_ReturnsEmail()
    {
        // Arrange
        // Создаем простой JWT токен с email claim
        var claims = new List<System.Security.Claims.Claim>
        {
            new System.Security.Claims.Claim(
                System.Security.Claims.ClaimTypes.Email,
                "test@example.com"
            ),
        };

        var token = new System.IdentityModel.Tokens.Jwt.JwtSecurityToken(
            issuer: "test",
            audience: "test",
            claims: claims,
            expires: DateTime.UtcNow.AddHours(1)
        );

        var tokenHandler = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler();
        var tokenString = tokenHandler.WriteToken(token);

        _mockLocalStorage.Setup(x => x.GetItem<string>("accessToken")).Returns(tokenString);

        // Act
        var result = _authService.GetUserEmailFromToken();

        // Assert
        Assert.Equal("test@example.com", result);
    }

    [Fact]
    public void GetUserEmailFromToken_WhenNoToken_ReturnsNull()
    {
        // Arrange
        _mockLocalStorage.Setup(x => x.GetItem<string>("accessToken")).Returns((string?)null);

        // Act
        var result = _authService.GetUserEmailFromToken();

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void GetUserEmailFromToken_WhenInvalidToken_ReturnsNull()
    {
        // Arrange
        var invalidToken = "invalid.token.here";
        _mockLocalStorage.Setup(x => x.GetItem<string>("accessToken")).Returns(invalidToken);

        // Act
        var result = _authService.GetUserEmailFromToken();

        // Assert
        Assert.Null(result);
    }

    private static HttpClient CreateTestHttpClient()
    {
        // Создаем HttpClient с базовым URL для тестов
        return new HttpClient { BaseAddress = new Uri("http://localhost:5000") };
    }
}

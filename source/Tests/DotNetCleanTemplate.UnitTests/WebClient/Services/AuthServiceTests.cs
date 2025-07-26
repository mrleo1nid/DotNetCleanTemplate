using DotNetCleanTemplate.Client.Services;
using DotNetCleanTemplate.Shared.Common;
using DotNetCleanTemplate.Shared.DTOs;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Moq.Protected;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text.Json;

namespace DotNetCleanTemplate.UnitTests.WebClient.Services;

public class AuthServiceTests
{
    private readonly Mock<ILocalStorageService> _mockLocalStorage;
    private readonly Mock<ILogger<AuthService>> _mockLogger;
    private readonly JsonSerializerOptions _jsonOptions;
    private readonly AuthService _authService;
    private readonly Mock<HttpMessageHandler> _mockHttpHandler;

    public AuthServiceTests()
    {
        _mockLocalStorage = new Mock<ILocalStorageService>();
        _mockLogger = new Mock<ILogger<AuthService>>();
        _jsonOptions = new JsonSerializerOptions();
        _mockHttpHandler = new Mock<HttpMessageHandler>();

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

    #region LoginAsync Tests

    [Fact]
    public async Task LoginAsync_WithValidCredentials_ShouldReturnSuccess()
    {
        // Arrange
        var email = "test@example.com";
        var password = "password";
        var loginResponse = new LoginResponseDto
        {
            AccessToken = "access_token",
            RefreshToken = "refresh_token",
            RefreshTokenExpires = DateTime.UtcNow.AddDays(7),
        };

        var result = Result<LoginResponseDto>.Success(loginResponse);

        SetupHttpHandler(HttpStatusCode.OK, result);

        // Act
        var loginResult = await _authService.LoginAsync(email, password);

        // Assert
        Assert.Equal(LoginResult.Success, loginResult);
        _mockLocalStorage.Verify(x => x.SetItemAsync("accessToken", "access_token"), Times.Once);
        _mockLocalStorage.Verify(x => x.SetItemAsync("refreshToken", "refresh_token"), Times.Once);
        _mockLocalStorage.Verify(
            x => x.SetItemAsync("refreshTokenExpires", It.IsAny<string>()),
            Times.Once
        );
    }

    [Fact]
    public async Task LoginAsync_WithInvalidCredentials_ShouldReturnInvalidCredentials()
    {
        // Arrange
        var email = "test@example.com";
        var password = "wrong_password";

        SetupHttpHandler(HttpStatusCode.Unauthorized, null);

        // Act
        var result = await _authService.LoginAsync(email, password);

        // Assert
        Assert.Equal(LoginResult.InvalidCredentials, result);
    }

    [Fact]
    public async Task LoginAsync_WhenServerUnavailable_ShouldReturnServerUnavailable()
    {
        // Arrange
        var email = "test@example.com";
        var password = "password";

        SetupHttpHandler(HttpStatusCode.ServiceUnavailable, null);

        // Act
        var result = await _authService.LoginAsync(email, password);

        // Assert
        Assert.Equal(LoginResult.ServerUnavailable, result);
    }

    [Fact]
    public async Task LoginAsync_WhenGatewayTimeout_ShouldReturnServerUnavailable()
    {
        // Arrange
        var email = "test@example.com";
        var password = "password";

        SetupHttpHandler(HttpStatusCode.GatewayTimeout, null);

        // Act
        var result = await _authService.LoginAsync(email, password);

        // Assert
        Assert.Equal(LoginResult.ServerUnavailable, result);
    }

    [Fact]
    public async Task LoginAsync_WhenBadGateway_ShouldReturnServerUnavailable()
    {
        // Arrange
        var email = "test@example.com";
        var password = "password";

        SetupHttpHandler(HttpStatusCode.BadGateway, null);

        // Act
        var result = await _authService.LoginAsync(email, password);

        // Assert
        Assert.Equal(LoginResult.ServerUnavailable, result);
    }

    [Fact]
    public async Task LoginAsync_WhenUnexpectedStatusCode_ShouldReturnUnknownError()
    {
        // Arrange
        var email = "test@example.com";
        var password = "password";

        SetupHttpHandler(HttpStatusCode.InternalServerError, null);

        // Act
        var result = await _authService.LoginAsync(email, password);

        // Assert
        Assert.Equal(LoginResult.UnknownError, result);
    }

    [Fact]
    public async Task LoginAsync_WhenHttpRequestException_ShouldReturnNetworkError()
    {
        // Arrange
        var email = "test@example.com";
        var password = "password";

        _mockHttpHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ThrowsAsync(new HttpRequestException("Network error"));

        // Act
        var result = await _authService.LoginAsync(email, password);

        // Assert
        Assert.Equal(LoginResult.NetworkError, result);
    }

    [Fact]
    public async Task LoginAsync_WhenTaskCanceledException_ShouldReturnNetworkError()
    {
        // Arrange
        var email = "test@example.com";
        var password = "password";

        _mockHttpHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ThrowsAsync(new TaskCanceledException("Timeout"));

        // Act
        var result = await _authService.LoginAsync(email, password);

        // Assert
        Assert.Equal(LoginResult.NetworkError, result);
    }

    [Fact]
    public async Task LoginAsync_WhenSuccessResponseButInvalidResult_ShouldReturnInvalidCredentials()
    {
        // Arrange
        var email = "test@example.com";
        var password = "password";

        var result = Result<LoginResponseDto>.Failure("Login.Failed", "Invalid credentials");

        SetupHttpHandler(HttpStatusCode.OK, result);

        // Act
        var loginResult = await _authService.LoginAsync(email, password);

        // Assert
        Assert.Equal(LoginResult.InvalidCredentials, loginResult);
    }

    #endregion

    #region RefreshTokenAsync Tests

    [Fact]
    public async Task RefreshTokenAsync_WithValidToken_ShouldReturnSuccess()
    {
        // Arrange
        var refreshResponse = new RefreshTokenResponseDto
        {
            AccessToken = "new_access_token",
            RefreshToken = "new_refresh_token",
            Expires = DateTime.UtcNow.AddDays(7),
        };

        var result = Result<RefreshTokenResponseDto>.Success(refreshResponse);

        _mockLocalStorage
            .Setup(x => x.GetItemAsync<string>("refreshToken"))
            .ReturnsAsync("valid_refresh_token");

        SetupHttpHandler(HttpStatusCode.OK, result);

        // Act
        var refreshResult = await _authService.RefreshTokenAsync();

        // Assert
        Assert.Equal(RefreshTokenResult.Success, refreshResult);
        _mockLocalStorage.Verify(
            x => x.SetItemAsync("accessToken", "new_access_token"),
            Times.Once
        );
        _mockLocalStorage.Verify(
            x => x.SetItemAsync("refreshToken", "new_refresh_token"),
            Times.Once
        );
        _mockLocalStorage.Verify(
            x => x.SetItemAsync("refreshTokenExpires", It.IsAny<string>()),
            Times.Once
        );
    }

    [Fact]
    public async Task RefreshTokenAsync_WhenTokenNotFound_ShouldReturnTokenNotFound()
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
    public async Task RefreshTokenAsync_WhenTokenExpired_ShouldReturnTokenExpired()
    {
        // Arrange
        _mockLocalStorage
            .Setup(x => x.GetItemAsync<string>("refreshToken"))
            .ReturnsAsync("expired_refresh_token");

        SetupHttpHandler(HttpStatusCode.Unauthorized, null);

        // Act
        var result = await _authService.RefreshTokenAsync();

        // Assert
        Assert.Equal(RefreshTokenResult.TokenExpired, result);
    }

    [Fact]
    public async Task RefreshTokenAsync_WhenServerUnavailable_ShouldReturnServerUnavailable()
    {
        // Arrange
        _mockLocalStorage
            .Setup(x => x.GetItemAsync<string>("refreshToken"))
            .ReturnsAsync("valid_refresh_token");

        SetupHttpHandler(HttpStatusCode.ServiceUnavailable, null);

        // Act
        var result = await _authService.RefreshTokenAsync();

        // Assert
        Assert.Equal(RefreshTokenResult.ServerUnavailable, result);
    }

    [Fact]
    public async Task RefreshTokenAsync_WhenHttpRequestException_ShouldReturnNetworkError()
    {
        // Arrange
        _mockLocalStorage
            .Setup(x => x.GetItemAsync<string>("refreshToken"))
            .ReturnsAsync("valid_refresh_token");

        _mockHttpHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ThrowsAsync(new HttpRequestException("Network error"));

        // Act
        var result = await _authService.RefreshTokenAsync();

        // Assert
        Assert.Equal(RefreshTokenResult.NetworkError, result);
    }

    [Fact]
    public async Task RefreshTokenAsync_WhenTaskCanceledException_ShouldReturnNetworkError()
    {
        // Arrange
        _mockLocalStorage
            .Setup(x => x.GetItemAsync<string>("refreshToken"))
            .ReturnsAsync("valid_refresh_token");

        _mockHttpHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ThrowsAsync(new TaskCanceledException("Timeout"));

        // Act
        var result = await _authService.RefreshTokenAsync();

        // Assert
        Assert.Equal(RefreshTokenResult.NetworkError, result);
    }

    [Fact]
    public async Task RefreshTokenAsync_WhenSuccessResponseButInvalidResult_ShouldReturnTokenExpired()
    {
        // Arrange
        _mockLocalStorage
            .Setup(x => x.GetItemAsync<string>("refreshToken"))
            .ReturnsAsync("valid_refresh_token");

        var result = Result<RefreshTokenResponseDto>.Failure(
            "RefreshToken.Failed",
            "Token expired"
        );

        SetupHttpHandler(HttpStatusCode.OK, result);

        // Act
        var refreshResult = await _authService.RefreshTokenAsync();

        // Assert
        Assert.Equal(RefreshTokenResult.TokenExpired, refreshResult);
    }

    [Fact]
    public async Task RefreshTokenAsync_WhenLocalStorageThrowsException_ShouldReturnUnknownError()
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

    #endregion

    #region LogoutAsync Tests

    [Fact]
    public async Task LogoutAsync_ShouldClearAllTokens()
    {
        // Act
        await _authService.LogoutAsync();

        // Assert
        _mockLocalStorage.Verify(x => x.RemoveItemAsync("accessToken"), Times.Once);
        _mockLocalStorage.Verify(x => x.RemoveItemAsync("refreshToken"), Times.Once);
        _mockLocalStorage.Verify(x => x.RemoveItemAsync("refreshTokenExpires"), Times.Once);
    }

    [Fact]
    public async Task LogoutAsync_WhenLocalStorageThrowsException_ShouldLogError()
    {
        // Arrange
        _mockLocalStorage
            .Setup(x => x.RemoveItemAsync("accessToken"))
            .ThrowsAsync(new Exception("Storage error"));

        // Act
        await _authService.LogoutAsync();

        // Assert
        _mockLogger.Verify(
            x =>
                x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => true),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()
                ),
            Times.Once
        );
    }

    #endregion

    #region IsAuthenticatedAsync Tests

    [Fact]
    public async Task IsAuthenticatedAsync_WhenNoAccessToken_ShouldReturnFalse()
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
    public async Task IsAuthenticatedAsync_WhenTokenExpired_ShouldAttemptRefresh()
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
    public async Task IsAuthenticatedAsync_WhenTokenValid_ShouldReturnTrue()
    {
        // Arrange
        var validToken = CreateValidJwtToken();
        _mockLocalStorage
            .Setup(x => x.GetItemAsync<string>("accessToken"))
            .ReturnsAsync(validToken);
        _mockLocalStorage
            .Setup(x => x.GetItem<string>("refreshTokenExpires"))
            .Returns(DateTime.UtcNow.AddDays(1).ToString("O")); // Валидный токен

        // Act
        var result = await _authService.IsAuthenticatedAsync();

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task IsAuthenticatedAsync_WhenRefreshSucceeds_ShouldReturnTrue()
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
            .ReturnsAsync("valid_refresh_token");

        var refreshResponse = new RefreshTokenResponseDto
        {
            AccessToken = "new_access_token",
            RefreshToken = "new_refresh_token",
            Expires = DateTime.UtcNow.AddDays(7),
        };

        var result = Result<RefreshTokenResponseDto>.Success(refreshResponse);

        SetupHttpHandler(HttpStatusCode.OK, result);

        // Act
        var authResult = await _authService.IsAuthenticatedAsync();

        // Assert
        Assert.True(authResult);
    }

    [Fact]
    public async Task IsAuthenticatedAsync_WhenExceptionOccurs_ShouldReturnFalse()
    {
        // Arrange
        _mockLocalStorage
            .Setup(x => x.GetItemAsync<string>("accessToken"))
            .ThrowsAsync(new Exception("Storage error"));

        // Act
        var result = await _authService.IsAuthenticatedAsync();

        // Assert
        Assert.False(result);
    }

    #endregion

    #region GetAccessToken Tests

    [Fact]
    public void GetAccessToken_WhenValidToken_ShouldReturnToken()
    {
        // Arrange
        var expectedToken = CreateValidJwtToken();
        _mockLocalStorage.Setup(x => x.GetItem<string>("accessToken")).Returns(expectedToken);

        // Act
        var result = _authService.GetAccessToken();

        // Assert
        Assert.Equal(expectedToken, result);
    }

    [Fact]
    public void GetAccessToken_WhenNoToken_ShouldReturnNull()
    {
        // Arrange
        _mockLocalStorage.Setup(x => x.GetItem<string>("accessToken")).Returns((string?)null);

        // Act
        var result = _authService.GetAccessToken();

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void GetAccessToken_WhenExceptionOccurs_ShouldReturnNull()
    {
        // Arrange
        _mockLocalStorage
            .Setup(x => x.GetItem<string>("accessToken"))
            .Throws(new Exception("Storage error"));

        // Act
        var result = _authService.GetAccessToken();

        // Assert
        Assert.Null(result);
    }

    #endregion

    #region GetRefreshToken Tests

    [Fact]
    public void GetRefreshToken_WhenValidToken_ShouldReturnToken()
    {
        // Arrange
        var expectedToken = CreateValidJwtToken();
        _mockLocalStorage.Setup(x => x.GetItem<string>("refreshToken")).Returns(expectedToken);

        // Act
        var result = _authService.GetRefreshToken();

        // Assert
        Assert.Equal(expectedToken, result);
    }

    [Fact]
    public void GetRefreshToken_WhenNoToken_ShouldReturnNull()
    {
        // Arrange
        _mockLocalStorage.Setup(x => x.GetItem<string>("refreshToken")).Returns((string?)null);

        // Act
        var result = _authService.GetRefreshToken();

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void GetRefreshToken_WhenExceptionOccurs_ShouldReturnNull()
    {
        // Arrange
        _mockLocalStorage
            .Setup(x => x.GetItem<string>("refreshToken"))
            .Throws(new Exception("Storage error"));

        // Act
        var result = _authService.GetRefreshToken();

        // Assert
        Assert.Null(result);
    }

    #endregion

    #region IsTokenExpired Tests

    [Fact]
    public void IsTokenExpired_WhenNoExpiryDate_ShouldReturnTrue()
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
    public void IsTokenExpired_WhenExpired_ShouldReturnTrue()
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
    public void IsTokenExpired_WhenValid_ShouldReturnFalse()
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
    public void IsTokenExpired_WhenInvalidDate_ShouldReturnTrue()
    {
        // Arrange
        _mockLocalStorage
            .Setup(x => x.GetItem<string>("refreshTokenExpires"))
            .Returns("invalid_date");

        // Act
        var result = _authService.IsTokenExpired();

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void IsTokenExpired_WhenExceptionOccurs_ShouldReturnTrue()
    {
        // Arrange
        _mockLocalStorage
            .Setup(x => x.GetItem<string>("refreshTokenExpires"))
            .Throws(new Exception("Storage error"));

        // Act
        var result = _authService.IsTokenExpired();

        // Assert
        Assert.True(result);
    }

    #endregion

    #region GetUserEmailFromToken Tests

    [Fact]
    public void GetUserEmailFromToken_WhenValidToken_ShouldReturnEmail()
    {
        // Arrange
        var token = CreateValidJwtTokenWithEmail("test@example.com");
        _mockLocalStorage.Setup(x => x.GetItem<string>("accessToken")).Returns(token);

        // Act
        var result = _authService.GetUserEmailFromToken();

        // Assert
        Assert.Equal("test@example.com", result);
    }

    [Fact]
    public void GetUserEmailFromToken_WhenNoToken_ShouldReturnNull()
    {
        // Arrange
        _mockLocalStorage.Setup(x => x.GetItem<string>("accessToken")).Returns((string?)null);

        // Act
        var result = _authService.GetUserEmailFromToken();

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void GetUserEmailFromToken_WhenInvalidToken_ShouldReturnNull()
    {
        // Arrange
        var invalidToken = "invalid.token.here";
        _mockLocalStorage.Setup(x => x.GetItem<string>("accessToken")).Returns(invalidToken);

        // Act
        var result = _authService.GetUserEmailFromToken();

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void GetUserEmailFromToken_WhenTokenWithoutEmailClaim_ShouldReturnNull()
    {
        // Arrange
        var claims = new List<Claim> { new Claim(ClaimTypes.Name, "testuser") };

        var token = new JwtSecurityToken(
            issuer: "test",
            audience: "test",
            claims: claims,
            expires: DateTime.UtcNow.AddHours(1)
        );

        var tokenHandler = new JwtSecurityTokenHandler();
        var tokenString = tokenHandler.WriteToken(token);

        _mockLocalStorage.Setup(x => x.GetItem<string>("accessToken")).Returns(tokenString);

        // Act
        var result = _authService.GetUserEmailFromToken();

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void GetUserEmailFromToken_WhenExceptionOccurs_ShouldReturnNull()
    {
        // Arrange
        _mockLocalStorage
            .Setup(x => x.GetItem<string>("accessToken"))
            .Throws(new Exception("Storage error"));

        // Act
        var result = _authService.GetUserEmailFromToken();

        // Assert
        Assert.Null(result);
    }

    #endregion

    #region Helper Methods

    private static string CreateValidJwtToken()
    {
        var claims = new List<Claim> { new Claim(ClaimTypes.Name, "testuser") };

        var token = new JwtSecurityToken(
            issuer: "test",
            audience: "test",
            claims: claims,
            expires: DateTime.UtcNow.AddHours(1)
        );

        var tokenHandler = new JwtSecurityTokenHandler();
        return tokenHandler.WriteToken(token);
    }

    private static string CreateValidJwtTokenWithEmail(string email)
    {
        var claims = new List<Claim> { new Claim(ClaimTypes.Email, email) };

        var token = new JwtSecurityToken(
            issuer: "test",
            audience: "test",
            claims: claims,
            expires: DateTime.UtcNow.AddHours(1)
        );

        var tokenHandler = new JwtSecurityTokenHandler();
        return tokenHandler.WriteToken(token);
    }

    private void SetupHttpHandler(HttpStatusCode statusCode, object? content = null)
    {
        var response = new HttpResponseMessage(statusCode);

        if (content != null)
        {
            response.Content = JsonContent.Create(content, options: _jsonOptions);
        }

        _mockHttpHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(response);
    }

    private HttpClient CreateTestHttpClient()
    {
        return new HttpClient(_mockHttpHandler.Object)
        {
            BaseAddress = new Uri("http://localhost:5000"),
        };
    }

    #endregion
}

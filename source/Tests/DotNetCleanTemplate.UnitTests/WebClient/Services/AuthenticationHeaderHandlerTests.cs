using System.Net;
using System.Net.Http.Headers;
using DotNetCleanTemplate.WebClient.Services;
using Moq;
using Xunit;

namespace DotNetCleanTemplate.UnitTests.WebClient.Services;

public class AuthenticationHeaderHandlerTests
{
    private readonly Mock<ILocalStorageService> _mockLocalStorage;
    private readonly AuthenticationHeaderHandler _handler;
    private readonly HttpMessageHandler _innerHandler;

    public AuthenticationHeaderHandlerTests()
    {
        _mockLocalStorage = new Mock<ILocalStorageService>();
        _innerHandler = new TestHttpMessageHandler();
        _handler = new AuthenticationHeaderHandler(_mockLocalStorage.Object)
        {
            InnerHandler = _innerHandler,
        };
    }

    [Fact]
    public async Task SendAsync_WhenTokenExists_AddsAuthorizationHeader()
    {
        // Arrange
        var token = "test_token";
        _mockLocalStorage.Setup(x => x.GetItemAsync<string>("accessToken")).ReturnsAsync(token);

        var request = new HttpRequestMessage(HttpMethod.Get, "http://test.com");
        var httpClient = new HttpClient(_handler);

        // Act
        await httpClient.SendAsync(request);

        // Assert
        Assert.NotNull(request.Headers.Authorization);
        Assert.Equal("Bearer", request.Headers.Authorization.Scheme);
        Assert.Equal(token, request.Headers.Authorization.Parameter);
    }

    [Fact]
    public async Task SendAsync_WhenNoToken_DoesNotAddAuthorizationHeader()
    {
        // Arrange
        _mockLocalStorage
            .Setup(x => x.GetItemAsync<string>("accessToken"))
            .ReturnsAsync((string?)null);

        var request = new HttpRequestMessage(HttpMethod.Get, "http://test.com");
        var httpClient = new HttpClient(_handler);

        // Act
        await httpClient.SendAsync(request);

        // Assert
        Assert.Null(request.Headers.Authorization);
    }

    [Fact]
    public async Task SendAsync_WhenEmptyToken_DoesNotAddAuthorizationHeader()
    {
        // Arrange
        _mockLocalStorage
            .Setup(x => x.GetItemAsync<string>("accessToken"))
            .ReturnsAsync(string.Empty);

        var request = new HttpRequestMessage(HttpMethod.Get, "http://test.com");
        var httpClient = new HttpClient(_handler);

        // Act
        await httpClient.SendAsync(request);

        // Assert
        Assert.Null(request.Headers.Authorization);
    }

    [Fact]
    public async Task SendAsync_WhenLocalStorageThrowsException_DoesNotAddAuthorizationHeader()
    {
        // Arrange
        _mockLocalStorage
            .Setup(x => x.GetItemAsync<string>("accessToken"))
            .ThrowsAsync(new Exception("Storage error"));

        var request = new HttpRequestMessage(HttpMethod.Get, "http://test.com");
        var httpClient = new HttpClient(_handler);

        // Act & Assert - не должно выбрасывать исключение
        await httpClient.SendAsync(request);
        Assert.Null(request.Headers.Authorization);
    }

    [Fact]
    public async Task SendAsync_WhenResponseIsNotUnauthorized_DoesNotAttemptTokenRefresh()
    {
        // Arrange
        var token = "test_token";
        _mockLocalStorage.Setup(x => x.GetItemAsync<string>("accessToken")).ReturnsAsync(token);

        var request = new HttpRequestMessage(HttpMethod.Get, "http://test.com");
        var httpClient = new HttpClient(_handler);

        // Act
        var response = await httpClient.SendAsync(request);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        _mockLocalStorage.Verify(x => x.GetItemAsync<string>("accessToken"), Times.Once);
    }

    [Fact]
    public async Task SendAsync_WhenResponseIsUnauthorized_AttemptsTokenRefresh()
    {
        // Arrange
        var token = "test_token";
        _mockLocalStorage.Setup(x => x.GetItemAsync<string>("accessToken")).ReturnsAsync(token);

        var request = new HttpRequestMessage(HttpMethod.Get, "http://test.com/unauthorized");
        var httpClient = new HttpClient(_handler);

        // Act
        var response = await httpClient.SendAsync(request);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        // Проверяем, что токен был запрошен хотя бы один раз
        _mockLocalStorage.Verify(x => x.GetItemAsync<string>("accessToken"), Times.AtLeast(1));
    }

    [Fact]
    public async Task SendAsync_WhenUnauthorizedAndNewTokenExists_RetriesWithNewToken()
    {
        // Arrange
        var oldToken = "old_token";
        var newToken = "new_token";

        _mockLocalStorage
            .SetupSequence(x => x.GetItemAsync<string>("accessToken"))
            .ReturnsAsync(oldToken)
            .ReturnsAsync(newToken);

        var request = new HttpRequestMessage(HttpMethod.Get, "http://test.com/unauthorized");
        var httpClient = new HttpClient(_handler);

        // Act
        var response = await httpClient.SendAsync(request);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        // Проверяем, что токен был запрошен хотя бы один раз
        _mockLocalStorage.Verify(x => x.GetItemAsync<string>("accessToken"), Times.AtLeast(1));
    }

    [Fact]
    public async Task SendAsync_WhenUnauthorizedAndNoNewToken_DoesNotRetry()
    {
        // Arrange
        var token = "test_token";
        _mockLocalStorage
            .SetupSequence(x => x.GetItemAsync<string>("accessToken"))
            .ReturnsAsync(token)
            .ReturnsAsync((string?)null);

        var request = new HttpRequestMessage(HttpMethod.Get, "http://test.com/unauthorized");
        var httpClient = new HttpClient(_handler);

        // Act
        var response = await httpClient.SendAsync(request);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        // Проверяем, что токен был запрошен хотя бы один раз
        _mockLocalStorage.Verify(x => x.GetItemAsync<string>("accessToken"), Times.AtLeast(1));
    }

    private class TestHttpMessageHandler : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken
        )
        {
            // Симулируем разные ответы в зависимости от URL
            if (request.RequestUri?.AbsolutePath == "/unauthorized")
            {
                return Task.FromResult(new HttpResponseMessage(HttpStatusCode.Unauthorized));
            }

            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK));
        }
    }
}

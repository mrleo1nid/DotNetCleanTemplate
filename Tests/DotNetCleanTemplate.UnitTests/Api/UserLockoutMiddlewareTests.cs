using System.Security.Claims;
using System.Text.Json;
using DotNetCleanTemplate.Api.Handlers;
using DotNetCleanTemplate.Application.Interfaces;
using DotNetCleanTemplate.Shared.Common;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace DotNetCleanTemplate.UnitTests.Api
{
    public class UserLockoutMiddlewareTests
    {
        private readonly Mock<IUserLockoutService> _mockUserLockoutService;
        private readonly Mock<ILogger<UserLockoutMiddleware>> _mockLogger;
        private readonly UserLockoutMiddleware _middleware;
        private readonly DefaultHttpContext _httpContext;

        public UserLockoutMiddlewareTests()
        {
            _mockUserLockoutService = new Mock<IUserLockoutService>();
            _mockLogger = new Mock<ILogger<UserLockoutMiddleware>>();
            _middleware = new UserLockoutMiddleware(
                (context) => Task.CompletedTask,
                _mockLogger.Object
            );
            _httpContext = new DefaultHttpContext();
            _httpContext.Response.Body = new MemoryStream();
        }

        [Fact]
        public async Task InvokeAsync_WhenUserNotAuthenticated_ShouldContinueToNextMiddleware()
        {
            // Arrange
            _httpContext.User = new ClaimsPrincipal(new ClaimsIdentity());

            // Act
            await _middleware.InvokeAsync(_httpContext, _mockUserLockoutService.Object);

            // Assert
            _mockUserLockoutService.Verify(
                x => x.CheckUserLockoutAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()),
                Times.Never
            );
        }

        [Fact]
        public async Task InvokeAsync_WhenUserAuthenticatedButNoUserIdClaim_ShouldContinueToNextMiddleware()
        {
            // Arrange
            var claims = new List<Claim> { new Claim(ClaimTypes.Email, "test@example.com") };
            _httpContext.User = new ClaimsPrincipal(new ClaimsIdentity(claims, "Test"));

            // Act
            await _middleware.InvokeAsync(_httpContext, _mockUserLockoutService.Object);

            // Assert
            _mockUserLockoutService.Verify(
                x => x.CheckUserLockoutAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()),
                Times.Never
            );
        }

        [Fact]
        public async Task InvokeAsync_WhenUserAuthenticatedWithInvalidUserIdClaim_ShouldContinueToNextMiddleware()
        {
            // Arrange
            var claims = new List<Claim> { new Claim(ClaimTypes.NameIdentifier, "invalid-guid") };
            _httpContext.User = new ClaimsPrincipal(new ClaimsIdentity(claims, "Test"));

            // Act
            await _middleware.InvokeAsync(_httpContext, _mockUserLockoutService.Object);

            // Assert
            _mockUserLockoutService.Verify(
                x => x.CheckUserLockoutAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()),
                Times.Never
            );
        }

        [Fact]
        public async Task InvokeAsync_WhenUserAuthenticatedAndNotLocked_ShouldContinueToNextMiddleware()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
            };
            _httpContext.User = new ClaimsPrincipal(new ClaimsIdentity(claims, "Test"));

            _mockUserLockoutService
                .Setup(x => x.CheckUserLockoutAsync(userId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result<Unit>.Success());

            // Act
            await _middleware.InvokeAsync(_httpContext, _mockUserLockoutService.Object);

            // Assert
            _mockUserLockoutService.Verify(
                x => x.CheckUserLockoutAsync(userId, It.IsAny<CancellationToken>()),
                Times.Once
            );
        }

        [Fact]
        public async Task InvokeAsync_WhenUserAuthenticatedAndLocked_ShouldReturn403Forbidden()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
            };
            _httpContext.User = new ClaimsPrincipal(new ClaimsIdentity(claims, "Test"));

            var lockoutError = Result<Unit>.Failure(
                "Auth.UserLocked",
                "Account is temporarily locked due to multiple failed login attempts. Please try again later."
            );

            _mockUserLockoutService
                .Setup(x => x.CheckUserLockoutAsync(userId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(lockoutError);

            // Act
            await _middleware.InvokeAsync(_httpContext, _mockUserLockoutService.Object);

            // Assert
            Assert.Equal(StatusCodes.Status403Forbidden, _httpContext.Response.StatusCode);
            Assert.StartsWith("application/json", _httpContext.Response.ContentType);

            _httpContext.Response.Body.Position = 0;
            using var reader = new StreamReader(_httpContext.Response.Body);
            var responseBody = await reader.ReadToEndAsync();
            var errorResponse = JsonSerializer.Deserialize<Dictionary<string, string>>(
                responseBody
            );

            Assert.NotNull(errorResponse);
            Assert.Equal(
                "Account is temporarily locked due to multiple failed login attempts. Please try again later.",
                errorResponse["error"]
            );
            Assert.Equal("Auth.UserLocked", errorResponse["code"]);
        }

        [Fact]
        public async Task InvokeAsync_WhenUserLocked_ShouldLogWarning()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
            };
            _httpContext.User = new ClaimsPrincipal(new ClaimsIdentity(claims, "Test"));

            var lockoutError = Result<Unit>.Failure(
                "Auth.UserLocked",
                "Account is temporarily locked due to multiple failed login attempts. Please try again later."
            );

            _mockUserLockoutService
                .Setup(x => x.CheckUserLockoutAsync(userId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(lockoutError);

            // Act
            await _middleware.InvokeAsync(_httpContext, _mockUserLockoutService.Object);

            // Assert
            _mockLogger.Verify(
                x =>
                    x.Log(
                        LogLevel.Warning,
                        It.IsAny<EventId>(),
                        It.Is<It.IsAnyType>(
                            (v, t) =>
                                v != null
                                && v.ToString()!
                                    .Contains(
                                        $"Blocked user {userId} attempted to access protected endpoint"
                                    )
                        ),
                        It.IsAny<Exception>(),
                        It.IsAny<Func<It.IsAnyType, Exception?, string>>()
                    ),
                Times.Once
            );
        }
    }
}

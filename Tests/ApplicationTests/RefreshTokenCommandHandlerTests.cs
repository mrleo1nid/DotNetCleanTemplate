using DotNetCleanTemplate.Application.Features.Auth.Refresh;
using DotNetCleanTemplate.Domain.Entities;
using DotNetCleanTemplate.Domain.Repositories;
using DotNetCleanTemplate.Domain.Services;
using Moq;

namespace ApplicationTests
{
    public class RefreshTokenCommandHandlerTests
    {
        private static User CreateUser(Guid? id = null)
        {
            return new User(
                new DotNetCleanTemplate.Domain.ValueObjects.User.UserName("TestUser"),
                new DotNetCleanTemplate.Domain.ValueObjects.User.Email("test@example.com"),
                new DotNetCleanTemplate.Domain.ValueObjects.User.PasswordHash(
                    "12345678901234567890"
                )
            )
            {
                Id = id ?? Guid.NewGuid(),
            };
        }

        [Fact]
        public async Task Handle_SuccessfulRefresh_ReturnsNewTokens()
        {
            // Arrange
            var user = CreateUser();
            var refreshToken = new RefreshToken(
                "old-refresh",
                DateTime.UtcNow.AddDays(1),
                user.Id,
                "ip"
            );
            var userRepoMock = new Mock<IUserRepository>();
            userRepoMock.Setup(r => r.GetByIdAsync<User>(user.Id)).ReturnsAsync(user);
            var tokenServiceMock = new Mock<ITokenService>();
            tokenServiceMock
                .Setup(s =>
                    s.ValidateRefreshTokenAsync("old-refresh", It.IsAny<CancellationToken>())
                )
                .ReturnsAsync(refreshToken);
            tokenServiceMock.Setup(s => s.GenerateAccessToken(user)).Returns("access-token");
            tokenServiceMock
                .Setup(s =>
                    s.CreateAndStoreRefreshTokenAsync(
                        user,
                        refreshToken.CreatedByIp,
                        It.IsAny<CancellationToken>()
                    )
                )
                .ReturnsAsync(
                    new RefreshToken("new-refresh", DateTime.UtcNow.AddDays(7), user.Id, "ip")
                );
            var handler = new RefreshTokenCommandHandler(
                tokenServiceMock.Object,
                userRepoMock.Object
            );
            var command = new RefreshTokenCommand("old-refresh");

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal("new-refresh", result.Value.RefreshToken);
        }

        [Fact]
        public async Task Handle_InvalidRefreshToken_ReturnsFailure()
        {
            // Arrange
            var userRepoMock = new Mock<IUserRepository>();
            var tokenServiceMock = new Mock<ITokenService>();
            tokenServiceMock
                .Setup(s => s.ValidateRefreshTokenAsync("bad-token", It.IsAny<CancellationToken>()))
                .ThrowsAsync(new InvalidOperationException("Refresh token is invalid or revoked."));
            var handler = new RefreshTokenCommandHandler(
                tokenServiceMock.Object,
                userRepoMock.Object
            );
            var command = new RefreshTokenCommand("bad-token");

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Contains(
                "invalid or revoked",
                result.Errors[0].Message,
                StringComparison.OrdinalIgnoreCase
            );
        }

        [Fact]
        public async Task Handle_UserNotFound_ReturnsFailure()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var refreshToken = new RefreshToken(
                "refresh",
                DateTime.UtcNow.AddDays(1),
                userId,
                "ip"
            );
            var userRepoMock = new Mock<IUserRepository>();
            userRepoMock.Setup(r => r.GetByIdAsync<User>(userId)).ReturnsAsync((User?)null);
            var tokenServiceMock = new Mock<ITokenService>();
            tokenServiceMock
                .Setup(s => s.ValidateRefreshTokenAsync("refresh", It.IsAny<CancellationToken>()))
                .ReturnsAsync(refreshToken);
            var handler = new RefreshTokenCommandHandler(
                tokenServiceMock.Object,
                userRepoMock.Object
            );
            var command = new RefreshTokenCommand("refresh");

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Contains("User not found", result.Errors[0].Message);
        }
    }
}

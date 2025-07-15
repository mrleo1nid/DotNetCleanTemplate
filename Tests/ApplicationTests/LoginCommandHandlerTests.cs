using DotNetCleanTemplate.Application.Features.Auth.Login;
using DotNetCleanTemplate.Domain.Entities;
using DotNetCleanTemplate.Domain.Repositories;
using DotNetCleanTemplate.Domain.Services;
using DotNetCleanTemplate.Shared.DTOs;
using Moq;

namespace ApplicationTests
{
    public class LoginCommandHandlerTests
    {
        private static User CreateUser(
            string email = "test@example.com",
            string password = "12345678901234567890"
        )
        {
            return new User(
                new DotNetCleanTemplate.Domain.ValueObjects.User.UserName("TestUser"),
                new DotNetCleanTemplate.Domain.ValueObjects.User.Email(email),
                new DotNetCleanTemplate.Domain.ValueObjects.User.PasswordHash(password)
            );
        }

        [Fact]
        public async Task Handle_SuccessfulLogin_ReturnsTokens()
        {
            // Arrange
            var user = CreateUser();
            var userRepoMock = new Mock<IUserRepository>();
            userRepoMock
                .Setup(r => r.FindByEmailAsync(user.Email.Value, It.IsAny<CancellationToken>()))
                .ReturnsAsync(user);
            var tokenServiceMock = new Mock<ITokenService>();
            tokenServiceMock.Setup(s => s.GenerateAccessToken(user)).Returns("access-token");
            tokenServiceMock
                .Setup(s =>
                    s.CreateAndStoreRefreshTokenAsync(
                        user,
                        It.IsAny<string>(),
                        It.IsAny<CancellationToken>()
                    )
                )
                .ReturnsAsync(
                    new RefreshToken(
                        "refresh-token",
                        System.DateTime.UtcNow.AddDays(7),
                        user.Id,
                        "ip"
                    )
                );
            var handler = new LoginCommandHandler(userRepoMock.Object, tokenServiceMock.Object);
            var command = new LoginCommand(
                new LoginRequestDto { Email = user.Email.Value, Password = user.PasswordHash.Value }
            );

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal("access-token", result.Value.AccessToken);
            Assert.Equal("refresh-token", result.Value.RefreshToken);
        }

        [Fact]
        public async Task Handle_InvalidEmail_ReturnsFailure()
        {
            // Arrange
            var userRepoMock = new Mock<IUserRepository>();
            userRepoMock
                .Setup(r => r.FindByEmailAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((User?)null);
            var tokenServiceMock = new Mock<ITokenService>();
            var handler = new LoginCommandHandler(userRepoMock.Object, tokenServiceMock.Object);
            var command = new LoginCommand(
                new LoginRequestDto { Email = "notfound@example.com", Password = "123456" }
            );

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Contains("Invalid email or password", result.Errors[0].Message);
        }

        [Fact]
        public async Task Handle_InvalidPassword_ReturnsFailure()
        {
            // Arrange
            var user = CreateUser();
            var userRepoMock = new Mock<IUserRepository>();
            userRepoMock
                .Setup(r => r.FindByEmailAsync(user.Email.Value, It.IsAny<CancellationToken>()))
                .ReturnsAsync(user);
            var tokenServiceMock = new Mock<ITokenService>();
            var handler = new LoginCommandHandler(userRepoMock.Object, tokenServiceMock.Object);
            var command = new LoginCommand(
                new LoginRequestDto
                {
                    Email = user.Email.Value,
                    Password = "wrongpasswordwrongpass",
                }
            );

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Contains("Invalid email or password", result.Errors[0].Message);
        }
    }
}

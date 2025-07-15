using DotNetCleanTemplate.Domain.Entities;
using DotNetCleanTemplate.Domain.Repositories;
using DotNetCleanTemplate.Domain.ValueObjects.User;
using DotNetCleanTemplate.Infrastructure.Configurations;
using DotNetCleanTemplate.Infrastructure.Services;
using Microsoft.Extensions.Options;
using Moq;

namespace InfrastructureTests
{
    public class TokenServiceTests
    {
        private TokenService CreateService(
            JwtSettings? settings = null,
            Mock<IRefreshTokenRepository>? refreshTokenRepo = null,
            Mock<IUnitOfWork>? unitOfWork = null
        )
        {
            settings ??= new JwtSettings
            {
                Key = "mysupersecretkey1234567890123456",
                Issuer = "issuer",
                Audience = "audience",
                AccessTokenExpirationMinutes = 60,
                RefreshTokenExpirationDays = 7,
            };
            var options = Options.Create(settings);
            refreshTokenRepo ??= new Mock<IRefreshTokenRepository>();
            unitOfWork ??= new Mock<IUnitOfWork>();
            return new TokenService(options, refreshTokenRepo.Object, unitOfWork.Object);
        }

        private static User CreateUser()
        {
            return new User(
                new UserName("username"),
                new Email("test@example.com"),
                new PasswordHash("sdfrtyhgfd")
            );
        }

        [Fact]
        public void GenerateToken_ShouldReturnValidToken()
        {
            var service = CreateService();
            var user = CreateUser();
            var token = service.GenerateAccessToken(user);
            Assert.False(string.IsNullOrWhiteSpace(token));
        }

        [Fact]
        public void GenerateRefreshToken_ShouldReturnNonEmpty()
        {
            var service = CreateService();
            var token = service.GenerateRefreshToken();
            Assert.False(string.IsNullOrWhiteSpace(token));
        }

        [Fact]
        public async Task CreateAndStoreRefreshTokenAsync_ShouldReturnToken()
        {
            var refreshTokenRepo = new Mock<IRefreshTokenRepository>();
            var unitOfWork = new Mock<IUnitOfWork>();
            var service = CreateService(refreshTokenRepo: refreshTokenRepo, unitOfWork: unitOfWork);
            var user = CreateUser();
            var result = await service.CreateAndStoreRefreshTokenAsync(user, "127.0.0.1");
            Assert.NotNull(result);
            refreshTokenRepo.Verify(r => r.AddAsync(It.IsAny<RefreshToken>()), Times.Once);
            unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}

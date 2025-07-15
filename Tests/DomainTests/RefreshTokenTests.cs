using DotNetCleanTemplate.Domain.Entities;

namespace DomainTests
{
    public class RefreshTokenTests
    {
        private static Guid userId = Guid.NewGuid();
        private static string ip = "127.0.0.1";

        [Fact]
        public void Constructor_ValidData_InitializesCorrectly()
        {
            var token = new RefreshToken("token", DateTime.UtcNow.AddDays(1), userId, ip);
            Assert.Equal("token", token.Token);
            Assert.Equal(userId, token.UserId);
            Assert.Equal(ip, token.CreatedByIp);
            Assert.Null(token.RevokedAt);
            Assert.Null(token.RevokedByIp);
            Assert.Null(token.ReplacedByToken);
        }

        [Fact]
        public void IsActive_True_WhenNotRevokedAndNotExpired()
        {
            var token = new RefreshToken("token", DateTime.UtcNow.AddDays(1), userId, ip);
            Assert.True(token.IsActive);
        }

        [Fact]
        public void IsActive_False_WhenRevoked()
        {
            var token = new RefreshToken("token", DateTime.UtcNow.AddDays(1), userId, ip);
            token.Revoke(ip);
            Assert.False(token.IsActive);
        }

        [Fact]
        public void IsActive_False_WhenExpired()
        {
            var token = new RefreshToken("token", DateTime.UtcNow.AddDays(-1), userId, ip);
            Assert.False(token.IsActive);
        }

        [Fact]
        public void Revoke_SetsRevokedAtAndByIp()
        {
            var token = new RefreshToken("token", DateTime.UtcNow.AddDays(1), userId, ip);
            token.Revoke(ip);
            Assert.NotNull(token.RevokedAt);
            Assert.Equal(ip, token.RevokedByIp);
        }

        [Fact]
        public void Revoke_Throws_WhenAlreadyRevoked()
        {
            var token = new RefreshToken("token", DateTime.UtcNow.AddDays(1), userId, ip);
            token.Revoke(ip);
            Assert.Throws<InvalidOperationException>(() => token.Revoke(ip));
        }

        [Fact]
        public void Revoke_Throws_WhenExpired()
        {
            var token = new RefreshToken("token", DateTime.UtcNow.AddDays(-1), userId, ip);
            Assert.Throws<InvalidOperationException>(() => token.Revoke(ip));
        }

        [Fact]
        public void Replace_SetsReplacedByToken()
        {
            var token = new RefreshToken("token", DateTime.UtcNow.AddDays(1), userId, ip);
            token.Replace("new-token");
            Assert.Equal("new-token", token.ReplacedByToken);
        }

        [Fact]
        public void Replace_Throws_WhenTokenIsNullOrWhitespace()
        {
            var token = new RefreshToken("token", DateTime.UtcNow.AddDays(1), userId, ip);
            Assert.Throws<ArgumentNullException>(() => token.Replace(null!));
            Assert.Throws<ArgumentNullException>(() => token.Replace(" "));
        }

        [Fact]
        public void IsExpired_ShouldReturnTrue_WhenExpired()
        {
            var token = new RefreshToken(
                "token",
                DateTime.UtcNow.AddDays(-1),
                Guid.NewGuid(),
                "127.0.0.1"
            );
            Assert.True(token.IsExpired);
        }

        [Fact]
        public void IsExpired_ShouldReturnFalse_WhenNotExpired()
        {
            var token = new RefreshToken(
                "token",
                DateTime.UtcNow.AddDays(1),
                Guid.NewGuid(),
                "127.0.0.1"
            );
            Assert.False(token.IsExpired);
        }
    }
}

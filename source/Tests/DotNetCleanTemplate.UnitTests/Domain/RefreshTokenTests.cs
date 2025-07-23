using DotNetCleanTemplate.Domain.Entities;

namespace DotNetCleanTemplate.UnitTests.Domain
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
            var token = new RefreshToken(
                "token",
                DateTime.UtcNow.AddMinutes(10),
                Guid.NewGuid(),
                "ip"
            );
            token.Replace("newtoken");
            Assert.Equal("newtoken", token.ReplacedByToken);
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

        [Fact]
        public void IsExpired_And_IsActive_WorkCorrectly()
        {
            var notExpired = new RefreshToken(
                "token",
                DateTime.UtcNow.AddMinutes(10),
                Guid.NewGuid(),
                "ip"
            );
            Assert.False(notExpired.IsExpired);
            Assert.True(notExpired.IsActive);

            var expired = new RefreshToken(
                "token",
                DateTime.UtcNow.AddMinutes(-1),
                Guid.NewGuid(),
                "ip"
            );
            Assert.True(expired.IsExpired);
            Assert.False(expired.IsActive);
        }

        [Fact]
        public void Revoke_SetsRevokedAt_And_RevokedByIp()
        {
            var token = new RefreshToken(
                "token",
                DateTime.UtcNow.AddMinutes(10),
                Guid.NewGuid(),
                "ip"
            );
            token.Revoke("rip");
            Assert.NotNull(token.RevokedAt);
            Assert.Equal("rip", token.RevokedByIp);
            Assert.False(token.IsActive);
        }

        [Fact]
        public void Revoke_AlreadyRevoked_Throws()
        {
            var token = new RefreshToken(
                "token",
                DateTime.UtcNow.AddMinutes(10),
                Guid.NewGuid(),
                "ip"
            );
            token.Revoke("rip");
            Assert.Throws<InvalidOperationException>(() => token.Revoke("rip2"));
        }

        [Fact]
        public void Revoke_Expired_Throws()
        {
            var token = new RefreshToken(
                "token",
                DateTime.UtcNow.AddMinutes(-1),
                Guid.NewGuid(),
                "ip"
            );
            Assert.Throws<InvalidOperationException>(() => token.Revoke("rip"));
        }

        [Fact]
        public void Replace_Empty_Throws()
        {
            var token = new RefreshToken(
                "token",
                DateTime.UtcNow.AddMinutes(10),
                Guid.NewGuid(),
                "ip"
            );
            Assert.Throws<ArgumentNullException>(() => token.Replace(null!));
            Assert.Throws<ArgumentNullException>(() => token.Replace(""));
        }

        [Fact]
        public void Constructor_EmptyTokenOrIp_Throws()
        {
            Assert.Throws<ArgumentNullException>(() =>
                new RefreshToken(null!, DateTime.UtcNow, Guid.NewGuid(), "ip")
            );
            Assert.Throws<ArgumentNullException>(() =>
                new RefreshToken("token", DateTime.UtcNow, Guid.NewGuid(), null!)
            );
        }

        [Fact]
        public void RefreshToken_IsRevoked_WithRevokedToken()
        {
            var token = new RefreshToken(
                "token",
                DateTime.UtcNow.AddDays(1),
                Guid.NewGuid(),
                "127.0.0.1"
            );
            token.Revoke("127.0.0.1");
            Assert.NotNull(token.RevokedAt);
            Assert.False(token.IsActive);
        }

        [Fact]
        public void RefreshToken_IsRevoked_WithValidToken()
        {
            var token = new RefreshToken(
                "token",
                DateTime.UtcNow.AddDays(1),
                Guid.NewGuid(),
                "127.0.0.1"
            );
            Assert.Null(token.RevokedAt);
            Assert.True(token.IsActive);
        }
    }
}

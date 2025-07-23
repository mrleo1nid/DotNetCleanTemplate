using DotNetCleanTemplate.Domain.Entities;
using DotNetCleanTemplate.Domain.ValueObjects.User;
using DotNetCleanTemplate.Infrastructure.Persistent;
using DotNetCleanTemplate.Infrastructure.Persistent.Repositories;
using DotNetCleanTemplate.UnitTests.Common;
using Microsoft.EntityFrameworkCore;

namespace DotNetCleanTemplate.UnitTests.Infrastructure
{
    public class RefreshTokenRepositoryTests : RepositoryTestBase<AppDbContext>
    {
        private static User CreateTestUser()
        {
            return new User(
                new UserName("TestUser"),
                new Email("test@example.com"),
                new PasswordHash("12345678901234567890")
            );
        }

        [Fact]
        public async Task AddRefreshToken_ShouldAddToken()
        {
            var context = CreateDbContext(options => new AppDbContext(options));
            var user = CreateTestUser();
            context.Users.Add(user);
            await context.SaveChangesAsync();

            var repo = new RefreshTokenRepository(context);
            var token = new RefreshToken(
                "token1",
                DateTime.UtcNow.AddDays(1),
                user.Id,
                "127.0.0.1"
            );
            await repo.AddAsync(token);
            await context.SaveChangesAsync();

            var found = await context.RefreshTokens.FirstOrDefaultAsync(rt => rt.Token == "token1");
            Assert.NotNull(found);
            Assert.Equal("token1", found!.Token);
        }

        [Fact]
        public async Task GetByTokenAsync_ShouldReturnToken()
        {
            var context = CreateDbContext(options => new AppDbContext(options));
            var user = CreateTestUser();
            context.Users.Add(user);
            await context.SaveChangesAsync();

            var repo = new RefreshTokenRepository(context);
            var token = new RefreshToken(
                "token2",
                DateTime.UtcNow.AddDays(1),
                user.Id,
                "127.0.0.1"
            );
            await repo.AddAsync(token);
            await context.SaveChangesAsync();

            var found = await repo.FindByTokenAsync("token2");
            Assert.NotNull(found);
            Assert.Equal("token2", found!.Token);
        }

        [Fact]
        public async Task GetByUserIdAsync_ShouldReturnUserTokens()
        {
            var context = CreateDbContext(options => new AppDbContext(options));
            var user = CreateTestUser();
            context.Users.Add(user);
            await context.SaveChangesAsync();

            var repo = new RefreshTokenRepository(context);
            var token1 = new RefreshToken(
                "token3",
                DateTime.UtcNow.AddDays(1),
                user.Id,
                "127.0.0.1"
            );
            var token2 = new RefreshToken(
                "token4",
                DateTime.UtcNow.AddDays(1),
                user.Id,
                "127.0.0.1"
            );
            await repo.AddAsync(token1);
            await repo.AddAsync(token2);
            await context.SaveChangesAsync();

            var tokens = await repo.GetActiveTokensByUserIdAsync(user.Id);
            Assert.Equal(2, tokens.Count);
            Assert.Contains(tokens, t => t.Token == "token3");
            Assert.Contains(tokens, t => t.Token == "token4");
        }

        [Fact]
        public async Task UpdateAsync_ShouldUpdateToken()
        {
            var context = CreateDbContext(options => new AppDbContext(options));
            var user = CreateTestUser();
            context.Users.Add(user);
            await context.SaveChangesAsync();

            var repo = new RefreshTokenRepository(context);
            var token = new RefreshToken(
                "token5",
                DateTime.UtcNow.AddDays(1),
                user.Id,
                "127.0.0.1"
            );
            await repo.AddAsync(token);
            await context.SaveChangesAsync();

            token.Revoke("127.0.0.2");
            await repo.UpdateAsync(token);
            await context.SaveChangesAsync();

            var found = await repo.FindByTokenAsync("token5");
            Assert.NotNull(found);
            Assert.False(found!.IsActive);
            Assert.Equal("127.0.0.2", found.RevokedByIp);
        }

        [Fact]
        public async Task RemoveRefreshToken_ShouldRemoveToken()
        {
            var context = CreateDbContext(options => new AppDbContext(options));
            var user = CreateTestUser();
            context.Users.Add(user);
            var token = new RefreshToken(
                "token3",
                DateTime.UtcNow.AddDays(1),
                user.Id,
                "127.0.0.1"
            );
            context.RefreshTokens.Add(token);
            await context.SaveChangesAsync();

            var repo = new RefreshTokenRepository(context);
            await repo.DeleteAsync(token);
            await context.SaveChangesAsync();

            var found = await context.RefreshTokens.FirstOrDefaultAsync(rt => rt.Token == "token3");
            Assert.Null(found);
        }

        [Fact]
        public async Task FindByTokenAsync_ReturnsNull_WhenTokenNotFound()
        {
            var context = CreateDbContext(options => new AppDbContext(options));
            var repo = new RefreshTokenRepository(context);
            var found = await repo.FindByTokenAsync("not-exist");
            Assert.Null(found);
        }

        [Fact]
        public async Task GetActiveTokensByUserIdAsync_ReturnsOnlyActiveTokens()
        {
            var context = CreateDbContext(options => new AppDbContext(options));
            var user = CreateTestUser();
            context.Users.Add(user);
            var activeToken = new RefreshToken(
                "active",
                DateTime.UtcNow.AddDays(1),
                user.Id,
                "127.0.0.1"
            );
            var expiredToken = new RefreshToken(
                "expired",
                DateTime.UtcNow.AddDays(-1),
                user.Id,
                "127.0.0.1"
            );
            context.RefreshTokens.AddRange(activeToken, expiredToken);
            await context.SaveChangesAsync();

            var repo = new RefreshTokenRepository(context);
            var tokens = await repo.GetActiveTokensByUserIdAsync(user.Id);
            Assert.Single(tokens);
            Assert.Equal("active", tokens[0].Token);
        }

        [Fact]
        public async Task GetActiveTokensByUserIdAsync_ReturnsEmpty_WhenNoActiveTokens()
        {
            var context = CreateDbContext(options => new AppDbContext(options));
            var user = CreateTestUser();
            context.Users.Add(user);
            var expiredToken = new RefreshToken(
                "expired",
                DateTime.UtcNow.AddDays(-1),
                user.Id,
                "127.0.0.1"
            );
            context.RefreshTokens.Add(expiredToken);
            await context.SaveChangesAsync();

            var repo = new RefreshTokenRepository(context);
            var tokens = await repo.GetActiveTokensByUserIdAsync(user.Id);
            Assert.Empty(tokens);
        }

        [Fact]
        public async Task FindByToken_WithInvalidToken()
        {
            var context = CreateDbContext(options => new AppDbContext(options));
            var repo = new RefreshTokenRepository(context);

            var found = await repo.FindByTokenAsync("");
            Assert.Null(found);
        }

        [Fact]
        public async Task FindByToken_WithExpiredToken()
        {
            var context = CreateDbContext(options => new AppDbContext(options));
            var user = CreateTestUser();
            context.Users.Add(user);
            await context.SaveChangesAsync();

            var repo = new RefreshTokenRepository(context);
            var token = new RefreshToken(
                "expired-token",
                DateTime.UtcNow.AddDays(-1),
                user.Id,
                "127.0.0.1"
            );
            await repo.AddAsync(token);
            await context.SaveChangesAsync();

            var found = await repo.FindByTokenAsync("expired-token");
            Assert.NotNull(found);
            Assert.True(found!.IsExpired);
        }

        [Fact]
        public async Task FindByToken_WithRevokedToken()
        {
            var context = CreateDbContext(options => new AppDbContext(options));
            var user = CreateTestUser();
            context.Users.Add(user);
            await context.SaveChangesAsync();

            var repo = new RefreshTokenRepository(context);
            var token = new RefreshToken(
                "revoked-token",
                DateTime.UtcNow.AddDays(1),
                user.Id,
                "127.0.0.1"
            );
            await repo.AddAsync(token);
            await context.SaveChangesAsync();

            // Отзываем токен
            token.Revoke("127.0.0.2");
            await repo.UpdateAsync(token);
            await context.SaveChangesAsync();

            var found = await repo.FindByTokenAsync("revoked-token");
            Assert.NotNull(found);
            Assert.NotNull(found!.RevokedAt);
            Assert.False(found.IsActive);
        }

        [Fact]
        public async Task GetExpiredTokens_WithNoExpiredTokens()
        {
            var context = CreateDbContext(options => new AppDbContext(options));
            var user = CreateTestUser();
            context.Users.Add(user);
            await context.SaveChangesAsync();

            var repo = new RefreshTokenRepository(context);
            var activeToken = new RefreshToken(
                "active-token",
                DateTime.UtcNow.AddDays(1),
                user.Id,
                "127.0.0.1"
            );
            await repo.AddAsync(activeToken);
            await context.SaveChangesAsync();

            var expiredTokens = await repo.GetExpiredTokensAsync();
            Assert.Empty(expiredTokens);
        }

        [Fact]
        public async Task GetExpiredTokens_WithMultipleExpiredTokens()
        {
            var context = CreateDbContext(options => new AppDbContext(options));
            var user1 = CreateTestUser();
            var user2 = CreateTestUser();
            context.Users.AddRange(user1, user2);
            await context.SaveChangesAsync();

            var repo = new RefreshTokenRepository(context);
            var expiredToken1 = new RefreshToken(
                "expired1",
                DateTime.UtcNow.AddDays(-1),
                user1.Id,
                "127.0.0.1"
            );
            var expiredToken2 = new RefreshToken(
                "expired2",
                DateTime.UtcNow.AddDays(-2),
                user2.Id,
                "127.0.0.1"
            );
            var activeToken = new RefreshToken(
                "active",
                DateTime.UtcNow.AddDays(1),
                user1.Id,
                "127.0.0.1"
            );

            await repo.AddAsync(expiredToken1);
            await repo.AddAsync(expiredToken2);
            await repo.AddAsync(activeToken);
            await context.SaveChangesAsync();

            var expiredTokens = await repo.GetExpiredTokensAsync();
            Assert.Equal(2, expiredTokens.Count);
            Assert.Contains(expiredTokens, t => t.Token == "expired1");
            Assert.Contains(expiredTokens, t => t.Token == "expired2");
        }
    }
}

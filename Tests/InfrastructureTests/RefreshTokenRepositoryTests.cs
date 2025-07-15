using DotNetCleanTemplate.Domain.Entities;
using DotNetCleanTemplate.Domain.ValueObjects.User;
using DotNetCleanTemplate.Infrastructure.Persistent;
using DotNetCleanTemplate.Infrastructure.Persistent.Repositories;
using Microsoft.EntityFrameworkCore;

namespace InfrastructureTests
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

            var found = await context.RefreshTokens.FirstOrDefaultAsync(rt => rt.Token == "token1");
            Assert.NotNull(found);
            Assert.Equal(user.Id, found.UserId);
        }

        [Fact]
        public async Task GetRefreshToken_ShouldReturnToken()
        {
            var context = CreateDbContext(options => new AppDbContext(options));
            var user = CreateTestUser();
            context.Users.Add(user);
            var token = new RefreshToken(
                "token2",
                DateTime.UtcNow.AddDays(1),
                user.Id,
                "127.0.0.1"
            );
            context.RefreshTokens.Add(token);
            await context.SaveChangesAsync();

            var repo = new RefreshTokenRepository(context);
            var found = await repo.FindByTokenAsync("token2");
            Assert.NotNull(found);
            Assert.Equal(user.Id, found.UserId);
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

            var found = await context.RefreshTokens.FirstOrDefaultAsync(rt => rt.Token == "token3");
            Assert.Null(found);
        }
    }
}

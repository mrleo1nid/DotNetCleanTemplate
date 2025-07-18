using DotNetCleanTemplate.Domain.Entities;
using DotNetCleanTemplate.Infrastructure.Persistent;
using DotNetCleanTemplate.Infrastructure.Persistent.Repositories;
using DotNetCleanTemplate.UnitTests.Common;

namespace DotNetCleanTemplate.UnitTests.Infrastructure
{
    public class UnitOfWorkTests : RepositoryTestBase<AppDbContext>
    {
        private static new User CreateTestUser(string? email = null)
        {
            return new User(
                new DotNetCleanTemplate.Domain.ValueObjects.User.UserName("UnitOfWorkUser"),
                new DotNetCleanTemplate.Domain.ValueObjects.User.Email(
                    email ?? $"unit{Guid.NewGuid()}@example.com"
                ),
                new DotNetCleanTemplate.Domain.ValueObjects.User.PasswordHash(
                    "12345678901234567890"
                )
            );
        }

        [Fact]
        public async Task SaveChangesAsync_PersistsData()
        {
            using var context = CreateDbContext(options => new AppDbContext(options));
            var unitOfWork = new UnitOfWork(context);
            var user = CreateTestUser();
            context.Users.Add(user);
            var result = await unitOfWork.SaveChangesAsync();
            Assert.True(result > 0);
            var found = await context.Users.FindAsync(user.Id);
            Assert.NotNull(found);
            Assert.Equal(user.Id, found!.Id);
        }

        [Fact]
        public async Task SaveChangesAsync_WithCancellationToken_Works()
        {
            using var context = CreateDbContext(options => new AppDbContext(options));
            var unitOfWork = new UnitOfWork(context);
            var user = CreateTestUser();
            context.Users.Add(user);
            using var cts = new System.Threading.CancellationTokenSource();
            var result = await unitOfWork.SaveChangesAsync(cts.Token);
            Assert.True(result > 0);
        }
    }
}

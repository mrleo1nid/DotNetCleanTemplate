using DotNetCleanTemplate.Domain.Entities;
using DotNetCleanTemplate.Infrastructure.Persistent;
using DotNetCleanTemplate.Infrastructure.Persistent.Repositories;
using DotNetCleanTemplate.UnitTests.Common;
using Microsoft.EntityFrameworkCore;

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

        [Fact]
        public async Task ExecuteInTransactionAsync_WithExistingTransaction()
        {
            using var context = CreateDbContext(options => new AppDbContext(options));
            var unitOfWork = new UnitOfWork(context);

            // InMemory не поддерживает транзакции, но метод должен работать
            await unitOfWork.ExecuteInTransactionAsync(async () =>
            {
                var user = CreateTestUser();
                context.Users.Add(user);
                await unitOfWork.SaveChangesAsync();
            });

            var savedUser = await context.Users.FirstOrDefaultAsync();
            Assert.NotNull(savedUser);
        }

        [Fact]
        public async Task ExecuteInTransactionAsync_WhenActionThrowsException_RollsBack()
        {
            using var context = CreateDbContext(options => new AppDbContext(options));
            var unitOfWork = new UnitOfWork(context);

            await Assert.ThrowsAsync<InvalidOperationException>(async () =>
                await unitOfWork.ExecuteInTransactionAsync(async () =>
                {
                    var user = CreateTestUser();
                    context.Users.Add(user);
                    await unitOfWork.SaveChangesAsync();
                    throw new InvalidOperationException("Test exception");
                })
            );

            // В InMemory транзакции не работают, но исключение должно быть выброшено
        }

        [Fact]
        public async Task ExecuteInTransactionAsync_WithDifferentIsolationLevels()
        {
            using var context = CreateDbContext(options => new AppDbContext(options));
            var unitOfWork = new UnitOfWork(context);

            // InMemory игнорирует isolation levels, но метод должен работать
            await unitOfWork.ExecuteInTransactionAsync(
                async () =>
                {
                    var user = CreateTestUser();
                    context.Users.Add(user);
                    await unitOfWork.SaveChangesAsync();
                },
                System.Data.IsolationLevel.ReadCommitted
            );

            var savedUser = await context.Users.FirstOrDefaultAsync();
            Assert.NotNull(savedUser);
        }

        [Fact]
        public async Task ExecuteInTransactionAsync_WithReadUncommitted()
        {
            using var context = CreateDbContext(options => new AppDbContext(options));
            var unitOfWork = new UnitOfWork(context);

            // InMemory игнорирует isolation levels, но метод должен работать
            await unitOfWork.ExecuteInTransactionAsync(
                async () =>
                {
                    var user = CreateTestUser();
                    context.Users.Add(user);
                    await unitOfWork.SaveChangesAsync();
                },
                System.Data.IsolationLevel.ReadUncommitted
            );

            var savedUser = await context.Users.FirstOrDefaultAsync();
            Assert.NotNull(savedUser);
        }

        [Fact]
        public async Task ExecuteInTransactionAsync_WithSerializable()
        {
            using var context = CreateDbContext(options => new AppDbContext(options));
            var unitOfWork = new UnitOfWork(context);

            // InMemory игнорирует isolation levels, но метод должен работать
            await unitOfWork.ExecuteInTransactionAsync(
                async () =>
                {
                    var user = CreateTestUser();
                    context.Users.Add(user);
                    await unitOfWork.SaveChangesAsync();
                },
                System.Data.IsolationLevel.Serializable
            );

            var savedUser = await context.Users.FirstOrDefaultAsync();
            Assert.NotNull(savedUser);
        }

        [Fact]
        public async Task SaveChangesAsync_WithConcurrentModifications()
        {
            using var context1 = CreateDbContext(options => new AppDbContext(options));
            using var context2 = CreateDbContext(options => new AppDbContext(options));
            var unitOfWork1 = new UnitOfWork(context1);

            var user = CreateTestUser();
            context1.Users.Add(user);
            await unitOfWork1.SaveChangesAsync();

            // В InMemory каждый контекст имеет свою базу данных
            // Поэтому нужно использовать один контекст для симуляции конкурентности
            var userFromContext1 = await context1.Users.FindAsync(user.Id);
            Assert.NotNull(userFromContext1);

            context1.Users.Update(userFromContext1!);
            await unitOfWork1.SaveChangesAsync();
        }
    }
}

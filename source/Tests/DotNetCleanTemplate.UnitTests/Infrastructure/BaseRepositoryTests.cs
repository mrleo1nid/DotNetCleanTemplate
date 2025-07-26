using DotNetCleanTemplate.Domain.Entities;
using DotNetCleanTemplate.Infrastructure.Persistent;
using DotNetCleanTemplate.Infrastructure.Persistent.Repositories;
using DotNetCleanTemplate.UnitTests.Common;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using System.Reflection;

namespace DotNetCleanTemplate.UnitTests.Infrastructure
{
    public class BaseRepositoryTests : RepositoryTestBase<AppDbContext>
    {
        [Fact]
        public async Task AddOrUpdateAsync_WhenEntityExists()
        {
            using var context = CreateDbContext(options => new AppDbContext(options));
            var repository = new UserRepository(context);
            var user = CreateTestUser();

            // Добавляем пользователя
            await repository.AddAsync(user);
            await context.SaveChangesAsync();

            // Обновляем пользователя через reflection, так как метод protected
            var method = typeof(BaseRepository).GetMethod(
                "AddOrUpdateAsync",
                BindingFlags.NonPublic | BindingFlags.Instance
            );
            var updatedUser = CreateTestUser("updated@example.com");
            updatedUser.Id = user.Id;

            await (Task)
                method!
                    .MakeGenericMethod(typeof(User))
                    .Invoke(
                        repository,
                        new object[]
                        {
                            updatedUser,
                            (Expression<Func<User, bool>>)(u => u.Id == user.Id),
                        }
                    )!;
            await context.SaveChangesAsync();

            var savedUser = await context.Users.FindAsync(user.Id);
            Assert.NotNull(savedUser);
        }

        [Fact]
        public async Task AddOrUpdateAsync_WhenEntityDoesNotExist()
        {
            using var context = CreateDbContext(options => new AppDbContext(options));
            var repository = new UserRepository(context);
            var user = CreateTestUser();

            // Используем reflection для доступа к protected методу
            var method = typeof(BaseRepository).GetMethod(
                "AddOrUpdateAsync",
                BindingFlags.NonPublic | BindingFlags.Instance
            );

            await (Task)
                method!
                    .MakeGenericMethod(typeof(User))
                    .Invoke(
                        repository,
                        new object[]
                        {
                            user,
                            (Expression<Func<User, bool>>)(u => u.Email.Value == user.Email.Value),
                        }
                    )!;
            await context.SaveChangesAsync();

            var savedUser = await context.Users.FindAsync(user.Id);
            Assert.NotNull(savedUser);
        }

        [Fact]
        public async Task DeleteAsync_WithNonExistentEntity()
        {
            using var context = CreateDbContext(options => new AppDbContext(options));
            var repository = new UserRepository(context);
            var nonExistentId = Guid.NewGuid();

            // Создаем фиктивную сущность для удаления
            var dummyUser = CreateTestUser();
            dummyUser.Id = nonExistentId;

            // Должно выбросить исключение при попытке удалить несуществующую сущность
            await Assert.ThrowsAsync<DbUpdateConcurrencyException>(async () =>
            {
                await repository.DeleteAsync(dummyUser);
                await context.SaveChangesAsync();
            });
        }

        [Fact]
        public async Task UpdateAsync_WithDetachedEntity()
        {
            using var context = CreateDbContext(options => new AppDbContext(options));
            var repository = new UserRepository(context);
            var user = CreateTestUser();

            // Добавляем пользователя
            await repository.AddAsync(user);
            await context.SaveChangesAsync();

            // Создаем новый контекст для симуляции detached entity
            using var context2 = CreateDbContext(options => new AppDbContext(options));
            var repository2 = new UserRepository(context2);

            // Должно выбросить исключение при попытке обновить detached entity
            await Assert.ThrowsAsync<DbUpdateConcurrencyException>(async () =>
            {
                await repository2.UpdateAsync(user);
                await context2.SaveChangesAsync();
            });
        }

        [Fact]
        public async Task GetByIdAsync_WithNonExistentId()
        {
            using var context = CreateDbContext(options => new AppDbContext(options));
            var repository = new UserRepository(context);
            var nonExistentId = Guid.NewGuid();

            var result = await repository.GetByIdAsync<User>(nonExistentId);
            Assert.Null(result);
        }

        [Fact]
        public async Task ExistsAsync_WithComplexPredicate()
        {
            using var context = CreateDbContext(options => new AppDbContext(options));
            var repository = new UserRepository(context);
            var user = CreateTestUser("test@example.com");

            await repository.AddAsync(user);
            await context.SaveChangesAsync();

            var exists = await repository.ExistsAsync<User>(u =>
                u.Email.Value == "test@example.com"
            );
            Assert.True(exists);
        }

        [Fact]
        public async Task GetAllAsync_WithEmptyResult()
        {
            using var context = CreateDbContext(options => new AppDbContext(options));
            var repository = new UserRepository(context);

            var users = await repository.GetAllAsync<User>();
            Assert.Empty(users);
        }

        [Fact]
        public async Task CountAsync_WithPredicate()
        {
            using var context = CreateDbContext(options => new AppDbContext(options));
            var repository = new UserRepository(context);
            var user1 = CreateTestUser("user1@example.com");
            var user2 = CreateTestUser("user2@example.com");

            await repository.AddAsync(user1);
            await repository.AddAsync(user2);
            await context.SaveChangesAsync();

            var count = await repository.CountAsync<User>();
            Assert.Equal(2, count);
        }
    }
}

using System.Linq.Expressions;
using System.Reflection;
using DotNetCleanTemplate.Domain.Entities;
using DotNetCleanTemplate.Infrastructure.Persistent;
using DotNetCleanTemplate.Infrastructure.Persistent.Repositories;
using DotNetCleanTemplate.UnitTests.Common;
using Microsoft.EntityFrameworkCore;

namespace DotNetCleanTemplate.UnitTests.Infrastructure
{
    // Тестовый класс для тестирования BaseRepository<T>
    public class TestBaseRepository : BaseRepository<User>
    {
        public TestBaseRepository(AppDbContext context)
            : base(context) { }

        public async Task<User> TestAddOrUpdateAsync(
            User entity,
            Expression<Func<User, bool>> predicate
        )
        {
            return await AddOrUpdateAsync(entity, predicate);
        }
    }

    public class BaseRepositoryTests : RepositoryTestBase<AppDbContext>
    {
        [Fact]
        public async Task AddOrUpdateAsync_WhenEntityExists()
        {
            using var context = CreateDbContext(options => new AppDbContext(options));
            var repository = new TestBaseRepository(context);
            var user = CreateTestUser();

            // Добавляем пользователя
            await repository.AddAsync(user);
            await context.SaveChangesAsync();

            // Обновляем пользователя
            var updatedUser = CreateTestUser("updated@example.com");
            updatedUser.Id = user.Id;

            await repository.TestAddOrUpdateAsync(updatedUser, u => u.Id == user.Id);
            await context.SaveChangesAsync();

            var savedUser = await context.Users.FindAsync(user.Id);
            Assert.NotNull(savedUser);
        }

        [Fact]
        public async Task AddOrUpdateAsync_WhenEntityDoesNotExist()
        {
            using var context = CreateDbContext(options => new AppDbContext(options));
            var repository = new TestBaseRepository(context);
            var user = CreateTestUser();

            await repository.TestAddOrUpdateAsync(user, u => u.Email.Value == user.Email.Value);
            await context.SaveChangesAsync();

            var savedUser = await context.Users.FindAsync(user.Id);
            Assert.NotNull(savedUser);
        }

        [Fact]
        public async Task DeleteAsync_WithNonExistentEntity()
        {
            using var context = CreateDbContext(options => new AppDbContext(options));
            var repository = new TestBaseRepository(context);
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
            var repository = new TestBaseRepository(context);
            var user = CreateTestUser();

            // Добавляем пользователя
            await repository.AddAsync(user);
            await context.SaveChangesAsync();

            // Создаем новый контекст для симуляции detached entity
            using var context2 = CreateDbContext(options => new AppDbContext(options));
            var repository2 = new TestBaseRepository(context2);

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
            var repository = new TestBaseRepository(context);
            var nonExistentId = Guid.NewGuid();

            var result = await repository.GetByIdAsync(nonExistentId);
            Assert.Null(result);
        }

        [Fact]
        public async Task ExistsAsync_WithComplexPredicate()
        {
            using var context = CreateDbContext(options => new AppDbContext(options));
            var repository = new TestBaseRepository(context);
            var user = CreateTestUser("test@example.com");

            await repository.AddAsync(user);
            await context.SaveChangesAsync();

            var exists = await repository.ExistsAsync(u => u.Email.Value == "test@example.com");
            Assert.True(exists);
        }

        [Fact]
        public async Task GetAllAsync_WithEmptyResult()
        {
            using var context = CreateDbContext(options => new AppDbContext(options));
            var repository = new TestBaseRepository(context);

            var users = await repository.GetAllAsync();
            Assert.Empty(users);
        }

        [Fact]
        public async Task CountAsync_WithPredicate()
        {
            using var context = CreateDbContext(options => new AppDbContext(options));
            var repository = new TestBaseRepository(context);
            var user1 = CreateTestUser("user1@example.com");
            var user2 = CreateTestUser("user2@example.com");

            await repository.AddAsync(user1);
            await repository.AddAsync(user2);
            await context.SaveChangesAsync();

            var count = await repository.CountAsync();
            Assert.Equal(2, count);
        }
    }
}

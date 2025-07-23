using DotNetCleanTemplate.Domain.Entities;
using DotNetCleanTemplate.Domain.ValueObjects.User;
using DotNetCleanTemplate.Infrastructure.Persistent;
using DotNetCleanTemplate.Infrastructure.Persistent.Repositories;
using DotNetCleanTemplate.UnitTests.Common;

namespace DotNetCleanTemplate.UnitTests.Infrastructure
{
    public class UserRepositoryTests : RepositoryTestBase<AppDbContext>
    {
        [Fact]
        public async Task AddAndGetByIdAsync_Works()
        {
            using var context = CreateDbContext(options => new AppDbContext(options));
            var repo = new UserRepository(context);
            var user = CreateTestUser();
            await repo.AddAsync(user);
            await context.SaveChangesAsync();
            var found = await repo.GetByIdAsync<User>(user.Id);
            Assert.NotNull(found);
            Assert.Equal(user.Id, found!.Id);
        }

        [Fact]
        public async Task FindByEmailAsync_Works()
        {
            using var context = CreateDbContext(options => new AppDbContext(options));
            var repo = new UserRepository(context);
            var user = CreateTestUser();
            await repo.AddAsync(user);
            await context.SaveChangesAsync();
            var found = await repo.FindByEmailAsync(user.Email.Value, CancellationToken.None);
            Assert.NotNull(found);
            Assert.Equal(user.Email.Value, found!.Email.Value);
        }

        [Fact]
        public async Task UpdateAsync_Works()
        {
            using var context = CreateDbContext(options => new AppDbContext(options));
            var repo = new UserRepository(context);
            var user = CreateTestUser();
            await repo.AddAsync(user);
            await context.SaveChangesAsync();
            var found = await repo.GetByIdAsync<User>(user.Id);
            Assert.NotNull(found);
            // Change name
            var newName = new UserName("UpdatedName");
            found!.GetType().GetProperty("Name")!.SetValue(found, newName);
            await repo.UpdateAsync(found);
            await context.SaveChangesAsync();
            var updated = await repo.GetByIdAsync<User>(user.Id);
            Assert.Equal("UpdatedName", updated!.Name.Value);
        }

        [Fact]
        public async Task RemoveAsync_Works()
        {
            using var context = CreateDbContext(options => new AppDbContext(options));
            var repo = new UserRepository(context);
            var user = CreateTestUser();
            await repo.AddAsync(user);
            await context.SaveChangesAsync();
            await repo.DeleteAsync(user);
            await context.SaveChangesAsync();
            var found = await repo.GetByIdAsync<User>(user.Id);
            Assert.Null(found);
        }

        [Fact]
        public async Task ExistsAsync_ReturnsTrueAndFalse()
        {
            using var context = CreateDbContext(options => new AppDbContext(options));
            var repo = new UserRepository(context);
            var user = CreateTestUser();
            await repo.AddAsync(user);
            await context.SaveChangesAsync();
            var exists = await repo.ExistsAsync<User>(u => u.Id == user.Id);
            var notExists = await repo.ExistsAsync<User>(u => u.Id == Guid.NewGuid());
            Assert.True(exists);
            Assert.False(notExists);
        }

        [Fact]
        public async Task GetAllAsync_ReturnsAllEntities()
        {
            using var context = CreateDbContext(options => new AppDbContext(options));
            var repo = new UserRepository(context);
            var user1 = CreateTestUser();
            var user2 = CreateTestUser();
            await repo.AddAsync(user1);
            await repo.AddAsync(user2);
            await context.SaveChangesAsync();
            var all = await repo.GetAllAsync<User>();
            Assert.Contains(all, u => u.Id == user1.Id);
            Assert.Contains(all, u => u.Id == user2.Id);
            Assert.Equal(2, all.Count());
        }

        [Fact]
        public async Task GetAllAsync_WithPredicate_ReturnsFilteredEntities()
        {
            using var context = CreateDbContext(options => new AppDbContext(options));
            var repo = new UserRepository(context);
            var user1 = CreateTestUser("a@a.com");
            var user2 = CreateTestUser("b@b.com");
            await repo.AddAsync(user1);
            await repo.AddAsync(user2);
            await context.SaveChangesAsync();
            var filtered = await repo.GetAllAsync<User>(u => u.Email.Value == "a@a.com");
            Assert.Single(filtered);
            Assert.Equal("a@a.com", filtered.First().Email.Value);
        }

        [Fact]
        public async Task CountAsync_ReturnsCorrectCount()
        {
            using var context = CreateDbContext(options => new AppDbContext(options));
            var repo = new UserRepository(context);
            var user1 = CreateTestUser();
            var user2 = CreateTestUser();
            await repo.AddAsync(user1);
            await repo.AddAsync(user2);
            await context.SaveChangesAsync();
            var count = await repo.CountAsync<User>();
            Assert.Equal(2, count);
        }

        [Fact]
        public async Task GetUserWithRolesAsync_ReturnsUserWithRoles()
        {
            using var context = CreateDbContext(options => new AppDbContext(options));
            var repo = new UserRepository(context);
            var user = new User(
                new("user4"),
                new("user4@example.com"),
                new("12345678901234567890")
            );
            var role = new Role(new("dev"));
            context.Users.Add(user);
            context.Roles.Add(role);
            var userRole = new UserRole(user, role);
            context.Set<UserRole>().Add(userRole);
            await context.SaveChangesAsync();

            var result = await repo.GetUserWithRolesAsync(user.Id);
            Assert.NotNull(result);
            Assert.Single(result!.UserRoles);
            Assert.Equal(role.Id, result.UserRoles.First().RoleId);
            Assert.Equal("dev", result.UserRoles.First().Role.Name.Value);
        }

        [Fact]
        public async Task GetByIdAsync_ReturnsNull_WhenUserNotFound()
        {
            using var context = CreateDbContext(options => new AppDbContext(options));
            var repo = new UserRepository(context);
            var found = await repo.GetByIdAsync<User>(Guid.NewGuid());
            Assert.Null(found);
        }

        [Fact]
        public async Task GetAllAsync_ReturnsEmpty_WhenNoUsers()
        {
            using var context = CreateDbContext(options => new AppDbContext(options));
            var repo = new UserRepository(context);
            var all = await repo.GetAllAsync<User>();
            Assert.Empty(all);
        }

        [Fact]
        public async Task ExistsAsync_ReturnsFalse_WhenUserNotFound()
        {
            using var context = CreateDbContext(options => new AppDbContext(options));
            var repo = new UserRepository(context);
            var exists = await repo.ExistsAsync<User>(u => u.Id == Guid.NewGuid());
            Assert.False(exists);
        }

        [Fact]
        public async Task CountAsync_ReturnsZero_WhenNoUsers()
        {
            using var context = CreateDbContext(options => new AppDbContext(options));
            var repo = new UserRepository(context);
            var count = await repo.CountAsync<User>();
            Assert.Equal(0, count);
        }

        [Fact]
        public async Task DeleteAsync_RemovesUser()
        {
            using var context = CreateDbContext(options => new AppDbContext(options));
            var repo = new UserRepository(context);
            var user = new User(
                new("userDel"),
                new("del@example.com"),
                new("12345678901234567890")
            );
            await repo.AddAsync(user);
            await context.SaveChangesAsync();
            await repo.DeleteAsync(user);
            await context.SaveChangesAsync();
            var found = await repo.GetByIdAsync<User>(user.Id);
            Assert.Null(found);
        }

        [Fact]
        public async Task UpdateAsync_ChangesUser()
        {
            using var context = CreateDbContext(options => new AppDbContext(options));
            var repo = new UserRepository(context);
            var user = new User(
                new("userUpd"),
                new("upd@example.com"),
                new("12345678901234567890")
            );
            await repo.AddAsync(user);
            await context.SaveChangesAsync();
            // Используем рефлексию для изменения свойства Name
            typeof(User).GetProperty("Name")!.SetValue(user, new UserName("UpdatedName"));
            await repo.UpdateAsync(user);
            await context.SaveChangesAsync();
            var found = await repo.GetByIdAsync<User>(user.Id);
            Assert.NotNull(found);
            Assert.Equal("UpdatedName", found!.Name.Value);
        }

        [Fact]
        public async Task FindByEmailAsync_ReturnsNull_WhenNotFound()
        {
            using var context = CreateDbContext(options => new AppDbContext(options));
            var repo = new UserRepository(context);
            var found = await repo.FindByEmailAsync("notfound@example.com");
            Assert.Null(found);
        }
    }
}

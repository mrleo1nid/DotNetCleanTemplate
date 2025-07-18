using DotNetCleanTemplate.Domain.Entities;
using DotNetCleanTemplate.Infrastructure.Persistent;
using DotNetCleanTemplate.Infrastructure.Persistent.Repositories;
using Microsoft.EntityFrameworkCore;

namespace DotNetCleanTemplate.UnitTests.Infrastructure
{
    public class UserRoleRepositoryTests : RepositoryTestBase<AppDbContext>
    {
        [Fact]
        public async Task GetByUserIdAsync_ReturnsUserRoles()
        {
            using var context = CreateDbContext(options => new AppDbContext(options));
            var repo = new UserRoleRepository(context);
            var user = new User(
                new("user1"),
                new("user1@example.com"),
                new("12345678901234567890")
            );
            var role = new Role(new("admin"));
            context.Users.Add(user);
            context.Roles.Add(role);
            var userRole = new UserRole(user, role);
            context.Set<UserRole>().Add(userRole);
            await context.SaveChangesAsync();

            var result = await repo.GetByUserIdAsync(user.Id);
            Assert.Single(result);
            Assert.Equal(role.Id, result.First().RoleId);
        }

        [Fact]
        public async Task GetByRoleIdAsync_ReturnsUserRoles()
        {
            using var context = CreateDbContext(options => new AppDbContext(options));
            var repo = new UserRoleRepository(context);
            var user = new User(
                new("user2"),
                new("user2@example.com"),
                new("12345678901234567890")
            );
            var role = new Role(new("user"));
            context.Users.Add(user);
            context.Roles.Add(role);
            var userRole = new UserRole(user, role);
            context.Set<UserRole>().Add(userRole);
            await context.SaveChangesAsync();

            var result = await repo.GetByRoleIdAsync(role.Id);
            Assert.Single(result);
            Assert.Equal(user.Id, result.First().UserId);
        }

        [Fact]
        public async Task GetUserWithRolesAsync_ReturnsUserWithRoles()
        {
            using var context = CreateDbContext(options => new AppDbContext(options));
            var repo = new UserRoleRepository(context);
            var user = new User(
                new("user3"),
                new("user3@example.com"),
                new("12345678901234567890")
            );
            var role = new Role(new("manager"));
            context.Users.Add(user);
            context.Roles.Add(role);
            var userRole = new UserRole(user, role);
            context.Set<UserRole>().Add(userRole);
            await context.SaveChangesAsync();

            var result = await repo.GetUserWithRolesAsync(user.Id);
            Assert.NotNull(result);
            Assert.Single(result!.UserRoles);
            Assert.Equal(role.Id, result.UserRoles.First().RoleId);
            Assert.Equal("manager", result.UserRoles.First().Role.Name.Value);
        }

        [Fact]
        public async Task GetByUserIdAsync_ReturnsEmpty_WhenNoUserRoles()
        {
            using var context = CreateDbContext(options => new AppDbContext(options));
            var repo = new UserRoleRepository(context);
            var result = await repo.GetByUserIdAsync(Guid.NewGuid());
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetByRoleIdAsync_ReturnsEmpty_WhenNoUserRoles()
        {
            using var context = CreateDbContext(options => new AppDbContext(options));
            var repo = new UserRoleRepository(context);
            var result = await repo.GetByRoleIdAsync(Guid.NewGuid());
            Assert.Empty(result);
        }

        [Fact]
        public async Task AddAsync_AddsUserRole()
        {
            using var context = CreateDbContext(options => new AppDbContext(options));
            var repo = new UserRoleRepository(context);
            var user = new User(
                new("userAdd"),
                new("add@example.com"),
                new("12345678901234567890")
            );
            var role = new Role(new("roleAdd"));
            context.Users.Add(user);
            context.Roles.Add(role);
            await context.SaveChangesAsync();
            var userRole = new UserRole(user, role);
            await repo.AddAsync(userRole);
            var found = await repo.GetByUserIdAsync(user.Id);
            Assert.Single(found);
        }

        [Fact]
        public async Task UpdateAsync_UpdatesUserRole()
        {
            var userId = Guid.NewGuid();
            var roleId = Guid.NewGuid();
            var dbName = $"UpdateUserRoleTest_{Guid.NewGuid()}";
            DateTime updatedAt;
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(dbName)
                .Options;
            using (var context = new AppDbContext(options))
            {
                var repo = new UserRoleRepository(context);
                var user = new User(
                    new("userUpd"),
                    new("upd@example.com"),
                    new("12345678901234567890")
                );
                typeof(User).GetProperty("Id")!.SetValue(user, userId);
                var role = new Role(new("roleUpd"));
                typeof(Role).GetProperty("Id")!.SetValue(role, roleId);
                context.Users.Add(user);
                context.Roles.Add(role);
                await context.SaveChangesAsync();
                var userRole = new UserRole(user, role);
                await repo.AddAsync(userRole);
                var found = (await repo.GetByUserIdAsync(user.Id)).First();
                var newDate = DateTime.UtcNow.AddDays(1);
                found.SetUpdatedAtForTest(newDate);
                await repo.UpdateAsync(found);
                await repo.SaveChangesAsync();
                updatedAt = newDate;
            }
            using (var context = new AppDbContext(options))
            {
                var repo = new UserRoleRepository(context);
                var found = (await repo.GetByUserIdAsync(userId)).First();
                Assert.Equal(updatedAt, found.UpdatedAt);
            }
        }

        [Fact]
        public async Task DeleteAsync_RemovesUserRole()
        {
            using var context = CreateDbContext(options => new AppDbContext(options));
            var repo = new UserRoleRepository(context);
            var user = new User(
                new("userDel"),
                new("del@example.com"),
                new("12345678901234567890")
            );
            var role = new Role(new("roleDel"));
            context.Users.Add(user);
            context.Roles.Add(role);
            await context.SaveChangesAsync();
            var userRole = new UserRole(user, role);
            await repo.AddAsync(userRole);
            await repo.DeleteAsync(userRole);
            var found = await repo.GetByUserIdAsync(user.Id);
            Assert.Empty(found);
        }

        [Fact]
        public async Task SaveChangesAsync_ReturnsZero_WhenNoChanges()
        {
            using var context = CreateDbContext(options => new AppDbContext(options));
            var repo = new UserRoleRepository(context);
            var result = await repo.SaveChangesAsync();
            Assert.Equal(0, result);
        }

        [Fact]
        public async Task CascadeDelete_User_DeletesUserRoles()
        {
            using var context = CreateDbContext(options => new AppDbContext(options));
            var repo = new UserRoleRepository(context);
            var user = new User(
                new("userCascade"),
                new("cascade@example.com"),
                new("12345678901234567890")
            );
            var role = new Role(new("roleCascade"));
            context.Users.Add(user);
            context.Roles.Add(role);
            await context.SaveChangesAsync();
            var userRole = new UserRole(user, role);
            await repo.AddAsync(userRole);
            context.Users.Remove(user);
            await context.SaveChangesAsync();
            var found = await repo.GetByRoleIdAsync(role.Id);
            Assert.Empty(found);
        }

        [Fact]
        public async Task CascadeDelete_Role_DeletesUserRoles()
        {
            using var context = CreateDbContext(options => new AppDbContext(options));
            var repo = new UserRoleRepository(context);
            var user = new User(
                new("userCascade2"),
                new("cascade2@example.com"),
                new("12345678901234567890")
            );
            var role = new Role(new("roleCascade2"));
            context.Users.Add(user);
            context.Roles.Add(role);
            await context.SaveChangesAsync();
            var userRole = new UserRole(user, role);
            await repo.AddAsync(userRole);
            context.Roles.Remove(role);
            await context.SaveChangesAsync();
            var found = await repo.GetByUserIdAsync(user.Id);
            Assert.Empty(found);
        }
    }
}

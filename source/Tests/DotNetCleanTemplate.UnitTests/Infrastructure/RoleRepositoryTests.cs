using DotNetCleanTemplate.Domain.Entities;
using DotNetCleanTemplate.Domain.ValueObjects.Role;
using DotNetCleanTemplate.Infrastructure.Persistent;
using DotNetCleanTemplate.Infrastructure.Persistent.Repositories;
using DotNetCleanTemplate.UnitTests.Common;

namespace DotNetCleanTemplate.UnitTests.Infrastructure
{
    public class RoleRepositoryTests : RepositoryTestBase<AppDbContext>
    {
        private static new Role CreateTestRole(string? name = null)
        {
            return new Role(new RoleName(name ?? $"Role{Guid.NewGuid()}"));
        }

        [Fact]
        public async Task AddAndGetByIdAsync_Works()
        {
            using var context = CreateDbContext(options => new AppDbContext(options));
            var repo = new RoleRepository(context);
            var role = CreateTestRole();
            await repo.AddAsync(role);
            await context.SaveChangesAsync();
            var found = await repo.GetByIdAsync(role.Id);
            Assert.NotNull(found);
            Assert.Equal(role.Id, found!.Id);
        }

        [Fact]
        public async Task FindByNameAsync_Works()
        {
            using var context = CreateDbContext(options => new AppDbContext(options));
            var repo = new RoleRepository(context);
            var role = CreateTestRole("Admin");
            await repo.AddAsync(role);
            await context.SaveChangesAsync();
            var found = await repo.FindByNameAsync(role.Name.Value, CancellationToken.None);
            Assert.NotNull(found);
            Assert.Equal(role.Name.Value, found!.Name.Value);
        }

        [Fact]
        public async Task UpdateAsync_Works()
        {
            using var context = CreateDbContext(options => new AppDbContext(options));
            var repo = new RoleRepository(context);
            var role = CreateTestRole();
            await repo.AddAsync(role);
            await context.SaveChangesAsync();
            typeof(Role).GetProperty("Name")!.SetValue(role, new RoleName("UpdatedRole"));
            await repo.UpdateAsync(role);
            await context.SaveChangesAsync();
            var found = await repo.GetByIdAsync(role.Id);
            Assert.NotNull(found);
            Assert.Equal("UpdatedRole", found!.Name.Value);
        }

        [Fact]
        public async Task DeleteAsync_Works()
        {
            using var context = CreateDbContext(options => new AppDbContext(options));
            var repo = new RoleRepository(context);
            var role = CreateTestRole();
            await repo.AddAsync(role);
            await context.SaveChangesAsync();
            await repo.DeleteAsync(role);
            await context.SaveChangesAsync();
            var found = await repo.GetByIdAsync(role.Id);
            Assert.Null(found);
        }

        [Fact]
        public async Task ExistsAsync_Works()
        {
            using var context = CreateDbContext(options => new AppDbContext(options));
            var repo = new RoleRepository(context);
            var role = CreateTestRole();
            await repo.AddAsync(role);
            await context.SaveChangesAsync();
            var exists = await repo.ExistsAsync(r => r.Id == role.Id);
            var notExists = await repo.ExistsAsync(r => r.Id == Guid.NewGuid());
            Assert.True(exists);
            Assert.False(notExists);
        }

        [Fact]
        public async Task GetAllAsync_Works()
        {
            using var context = CreateDbContext(options => new AppDbContext(options));
            var repo = new RoleRepository(context);
            var role1 = CreateTestRole("Role1");
            var role2 = CreateTestRole("Role2");
            await repo.AddAsync(role1);
            await repo.AddAsync(role2);
            await context.SaveChangesAsync();
            var all = await repo.GetAllAsync();
            Assert.Contains(all, r => r.Id == role1.Id);
            Assert.Contains(all, r => r.Id == role2.Id);
        }

        [Fact]
        public async Task GetAllAsync_WithPredicate_Works()
        {
            using var context = CreateDbContext(options => new AppDbContext(options));
            var repo = new RoleRepository(context);
            var role1 = CreateTestRole("Admin");
            var role2 = CreateTestRole("User");
            await repo.AddAsync(role1);
            await repo.AddAsync(role2);
            await context.SaveChangesAsync();
            var adminRoles = await repo.GetAllAsync(r => r.Name.Value == "Admin");
            Assert.Single(adminRoles);
            Assert.Equal("Admin", adminRoles.First().Name.Value);
        }

        [Fact]
        public async Task CountAsync_Works()
        {
            using var context = CreateDbContext(options => new AppDbContext(options));
            var repo = new RoleRepository(context);
            var role1 = CreateTestRole("Role1");
            var role2 = CreateTestRole("Role2");
            await repo.AddAsync(role1);
            await repo.AddAsync(role2);
            await context.SaveChangesAsync();
            var count = await repo.CountAsync();
            Assert.Equal(2, count);
        }

        [Fact]
        public async Task GetUserWithRolesAsync_ReturnsUserWithRoles()
        {
            using var context = CreateDbContext(options => new AppDbContext(options));
            var repo = new RoleRepository(context);
            var user = CreateTestUser();
            var role = CreateTestRole();
            context.Users.Add(user);
            context.Roles.Add(role);
            var userRole = new UserRole(user, role);
            context.Set<UserRole>().Add(userRole);
            await context.SaveChangesAsync();

            var result = await repo.GetUserWithRolesAsync(user.Id);
            Assert.NotNull(result);
            Assert.Single(result!.UserRoles);
            Assert.Equal(role.Id, result.UserRoles.First().RoleId);
            Assert.Equal(role.Name.Value, result.UserRoles.First().Role.Name.Value);
        }

        [Fact]
        public async Task GetByIdAsync_ReturnsNull_WhenRoleNotFound()
        {
            using var context = CreateDbContext(options => new AppDbContext(options));
            var repo = new RoleRepository(context);
            var found = await repo.GetByIdAsync(Guid.NewGuid());
            Assert.Null(found);
        }

        [Fact]
        public async Task GetAllAsync_ReturnsEmpty_WhenNoRoles()
        {
            using var context = CreateDbContext(options => new AppDbContext(options));
            var repo = new RoleRepository(context);
            var all = await repo.GetAllAsync();
            Assert.Empty(all);
        }

        [Fact]
        public async Task ExistsAsync_ReturnsFalse_WhenRoleNotFound()
        {
            using var context = CreateDbContext(options => new AppDbContext(options));
            var repo = new RoleRepository(context);
            var exists = await repo.ExistsAsync(r => r.Id == Guid.NewGuid());
            Assert.False(exists);
        }

        [Fact]
        public async Task CountAsync_ReturnsZero_WhenNoRoles()
        {
            using var context = CreateDbContext(options => new AppDbContext(options));
            var repo = new RoleRepository(context);
            var count = await repo.CountAsync();
            Assert.Equal(0, count);
        }

        [Fact]
        public async Task GetRolesPaginatedAsync_ReturnsPaginatedResults()
        {
            using var context = CreateDbContext(options => new AppDbContext(options));
            var repo = new RoleRepository(context);
            var role1 = CreateTestRole("Role1");
            var role2 = CreateTestRole("Role2");
            var role3 = CreateTestRole("Role3");
            await repo.AddAsync(role1);
            await repo.AddAsync(role2);
            await repo.AddAsync(role3);
            await context.SaveChangesAsync();

            var (roles, totalCount) = await repo.GetRolesPaginatedAsync(1, 2);
            Assert.Equal(2, roles.Count);
            Assert.Equal(3, totalCount);
        }
    }
}

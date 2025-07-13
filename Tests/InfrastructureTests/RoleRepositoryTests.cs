using DotNetCleanTemplate.Domain.Entities;
using DotNetCleanTemplate.Domain.ValueObjects.Role;
using DotNetCleanTemplate.Infrastructure.Persistent;
using DotNetCleanTemplate.Infrastructure.Persistent.Repositories;

namespace InfrastructureTests
{
    public class RoleRepositoryTests : RepositoryTestBase<AppDbContext>
    {
        private static Role CreateTestRole(string? name = null)
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
            var found = await repo.GetByIdAsync<Role>(role.Id);
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
            var found = await repo.FindByNameAsync("Admin");
            Assert.NotNull(found);
            Assert.Equal("Admin", found!.Name.Value);
        }

        [Fact]
        public async Task UpdateAsync_Works()
        {
            using var context = CreateDbContext(options => new AppDbContext(options));
            var repo = new RoleRepository(context);
            var role = CreateTestRole();
            await repo.AddAsync(role);
            var found = await repo.GetByIdAsync<Role>(role.Id);
            Assert.NotNull(found);
            // Change name
            var newName = new RoleName("UpdatedRole");
            found!.GetType().GetProperty("Name")!.SetValue(found, newName);
            await repo.UpdateAsync(found);
            var updated = await repo.GetByIdAsync<Role>(role.Id);
            Assert.Equal("UpdatedRole", updated!.Name.Value);
        }

        [Fact]
        public async Task RemoveAsync_Works()
        {
            using var context = CreateDbContext(options => new AppDbContext(options));
            var repo = new RoleRepository(context);
            var role = CreateTestRole();
            await repo.AddAsync(role);
            await repo.DeleteAsync(role);
            var found = await repo.GetByIdAsync<Role>(role.Id);
            Assert.Null(found);
        }

        [Fact]
        public async Task GetUserWithRolesAsync_ReturnsUserWithRoles()
        {
            using var context = CreateDbContext(options => new AppDbContext(options));
            var repo = new RoleRepository(context);
            var user = new User(
                new("user5"),
                new("user5@example.com"),
                new("12345678901234567890")
            );
            var role = new Role(new("qa1"));
            context.Users.Add(user);
            context.Roles.Add(role);
            var userRole = new UserRole(user, role);
            context.Set<UserRole>().Add(userRole);
            await context.SaveChangesAsync();

            var result = await repo.GetUserWithRolesAsync(user.Id);
            Assert.NotNull(result);
            Assert.Single(result!.UserRoles);
            Assert.Equal(role.Id, result.UserRoles.First().RoleId);
            Assert.Equal("qa1", result.UserRoles.First().Role.Name.Value);
        }

        [Fact]
        public async Task GetByIdAsync_ReturnsNull_WhenRoleNotFound()
        {
            using var context = CreateDbContext(options => new AppDbContext(options));
            var repo = new RoleRepository(context);
            var found = await repo.GetByIdAsync<Role>(Guid.NewGuid());
            Assert.Null(found);
        }

        [Fact]
        public async Task GetAllAsync_ReturnsEmpty_WhenNoRoles()
        {
            using var context = CreateDbContext(options => new AppDbContext(options));
            var repo = new RoleRepository(context);
            var all = await repo.GetAllAsync<Role>();
            Assert.Empty(all);
        }

        [Fact]
        public async Task ExistsAsync_ReturnsFalse_WhenRoleNotFound()
        {
            using var context = CreateDbContext(options => new AppDbContext(options));
            var repo = new RoleRepository(context);
            var exists = await repo.ExistsAsync<Role>(r => r.Id == Guid.NewGuid());
            Assert.False(exists);
        }

        [Fact]
        public async Task CountAsync_ReturnsZero_WhenNoRoles()
        {
            using var context = CreateDbContext(options => new AppDbContext(options));
            var repo = new RoleRepository(context);
            var count = await repo.CountAsync<Role>();
            Assert.Equal(0, count);
        }

        [Fact]
        public async Task DeleteAsync_RemovesRole()
        {
            using var context = CreateDbContext(options => new AppDbContext(options));
            var repo = new RoleRepository(context);
            var role = new Role(new("roleDel"));
            await repo.AddAsync(role);
            await repo.DeleteAsync(role);
            var found = await repo.GetByIdAsync<Role>(role.Id);
            Assert.Null(found);
        }

        [Fact]
        public async Task UpdateAsync_ChangesRole()
        {
            using var context = CreateDbContext(options => new AppDbContext(options));
            var repo = new RoleRepository(context);
            var role = new Role(new("roleUpd"));
            await repo.AddAsync(role);
            var found = await repo.GetByIdAsync<Role>(role.Id);
            found!
                .GetType()
                .GetProperty("Name")!
                .SetValue(
                    found,
                    new DotNetCleanTemplate.Domain.ValueObjects.Role.RoleName("newRoleName")
                );
            await repo.UpdateAsync(found);
            var updated = await repo.GetByIdAsync<Role>(role.Id);
            Assert.Equal("newRoleName", updated!.Name.Value);
        }

        [Fact]
        public async Task SaveChangesAsync_ReturnsZero_WhenNoChanges()
        {
            using var context = CreateDbContext(options => new AppDbContext(options));
            var repo = new RoleRepository(context);
            var result = await repo.SaveChangesAsync();
            Assert.Equal(0, result);
        }

        [Fact]
        public async Task FindByNameAsync_ReturnsNull_WhenNotFound()
        {
            using var context = CreateDbContext(options => new AppDbContext(options));
            var repo = new RoleRepository(context);
            var found = await repo.FindByNameAsync("notfound");
            Assert.Null(found);
        }
    }
}

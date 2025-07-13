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
    }
}

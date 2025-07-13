using Microsoft.EntityFrameworkCore;
using DotNetCleanTemplate.Application.Services;
using DotNetCleanTemplate.Domain.Entities;
using DotNetCleanTemplate.Domain.Repositories;
using DotNetCleanTemplate.Domain.ValueObjects.Role;
using DotNetCleanTemplate.Infrastructure.Persistent;
using DotNetCleanTemplate.Infrastructure.Persistent.Repositories;

namespace ApplicationTests
{
    public class RoleServiceTests : TestBase
    {
        private static Role CreateTestRole(string? name = null)
        {
            return new Role(new RoleName(name ?? $"Role{Guid.NewGuid()}"));
        }

        [Fact]
        public async Task CreateRoleAsync_Works()
        {
            using var context = CreateDbContext();
            var roleRepository = new RoleRepository(context);
            var unitOfWork = new UnitOfWork(context);
            var service = new RoleService(roleRepository, unitOfWork);
            var role = CreateTestRole();
            var created = await service.CreateRoleAsync(role);
            Assert.NotNull(created);
            Assert.Equal(role.Name.Value, created.Name.Value);
        }

        [Fact]
        public async Task FindByNameAsync_ReturnsRole()
        {
            using var context = CreateDbContext();
            var roleRepository = new RoleRepository(context);
            var unitOfWork = new UnitOfWork(context);
            var service = new RoleService(roleRepository, unitOfWork);
            var role = CreateTestRole();
            await service.CreateRoleAsync(role);
            var found = await service.FindByNameAsync(role.Name.Value);
            Assert.NotNull(found);
            Assert.Equal(role.Name.Value, found!.Name.Value);
        }

        [Fact]
        public async Task FindByNameAsync_ReturnsNullForUnknownName()
        {
            using var context = CreateDbContext();
            var roleRepository = new RoleRepository(context);
            var unitOfWork = new UnitOfWork(context);
            var service = new RoleService(roleRepository, unitOfWork);
            var found = await service.FindByNameAsync("UnknownRole");
            Assert.Null(found);
        }
    }
}

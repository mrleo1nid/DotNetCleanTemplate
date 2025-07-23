using DotNetCleanTemplate.Application.Services;
using DotNetCleanTemplate.Domain.Entities;
using DotNetCleanTemplate.Domain.ValueObjects.Role;
using DotNetCleanTemplate.Infrastructure.Persistent.Repositories;
using DotNetCleanTemplate.UnitTests.Common;

namespace DotNetCleanTemplate.UnitTests.Application
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
            var result = await service.CreateRoleAsync(role);
            Assert.True(result.IsSuccess);
            Assert.Equal(role.Name.Value, result.Value.Name.Value);
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
            var result = await service.FindByNameAsync(role.Name.Value);
            Assert.True(result.IsSuccess);
            Assert.Equal(role.Name.Value, result.Value.Name.Value);
        }

        [Fact]
        public async Task FindByNameAsync_ReturnsFailureForUnknownName()
        {
            using var context = CreateDbContext();
            var roleRepository = new RoleRepository(context);
            var unitOfWork = new UnitOfWork(context);
            var service = new RoleService(roleRepository, unitOfWork);
            var result = await service.FindByNameAsync("UnknownRole");
            Assert.False(result.IsSuccess);
            Assert.Contains(result.Errors, e => e.Code == "Role.NotFound");
        }

        [Fact]
        public async Task GetAllRolesAsync_ReturnsAllRoles()
        {
            using var context = CreateDbContext();
            var roleRepository = new RoleRepository(context);
            var unitOfWork = new UnitOfWork(context);
            var service = new RoleService(roleRepository, unitOfWork);
            var role1 = CreateTestRole("Admin");
            var role2 = CreateTestRole("User");
            await service.CreateRoleAsync(role1);
            await service.CreateRoleAsync(role2);
            var result = await service.GetAllRolesAsync();
            Assert.True(result.IsSuccess);
            Assert.Contains(result.Value, r => r.Name.Value == "Admin");
            Assert.Contains(result.Value, r => r.Name.Value == "User");
        }
    }
}

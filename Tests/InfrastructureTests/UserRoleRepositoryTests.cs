using System;
using System.Linq;
using System.Threading.Tasks;
using DotNetCleanTemplate.Domain.Entities;
using DotNetCleanTemplate.Infrastructure.Persistent;
using DotNetCleanTemplate.Infrastructure.Persistent.Repositories;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace InfrastructureTests
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
    }
}

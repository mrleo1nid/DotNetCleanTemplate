using System;
using DotNetCleanTemplate.Domain.Entities;
using DotNetCleanTemplate.Domain.ValueObjects.Role;
using DotNetCleanTemplate.Domain.ValueObjects.User;
using Xunit;

namespace DomainTests
{
    public class UserRoleTests
    {
        [Fact]
        public void Constructor_SetsProperties()
        {
            var user = new User(
                new UserName("user"),
                new Email("user@example.com"),
                new PasswordHash("12345678901234567890")
            );
            var role = new Role(new RoleName("admin"));
            var userRole = new UserRole(user, role);
            Assert.Equal(user.Id, userRole.UserId);
            Assert.Equal(role.Id, userRole.RoleId);
            Assert.Equal(user, userRole.User);
            Assert.Equal(role, userRole.Role);
        }

        [Fact]
        public void EFConstructor_AllowsCreation()
        {
            var userRole = (UserRole)Activator.CreateInstance(typeof(UserRole), true)!;
            Assert.NotNull(userRole);
        }

#if DEBUG
        [Fact]
        public void SetUpdatedAtForTest_SetsValue()
        {
            var user = new User(
                new UserName("user"),
                new Email("user@example.com"),
                new PasswordHash("12345678901234567890")
            );
            var role = new Role(new RoleName("admin"));
            var userRole = new UserRole(user, role);
            var dt = DateTime.UtcNow;
            userRole.SetUpdatedAtForTest(dt);
            Assert.Equal(dt, userRole.UpdatedAt);
        }
#endif
    }
}

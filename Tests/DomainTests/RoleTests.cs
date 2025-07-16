using DotNetCleanTemplate.Domain.Entities;
using DotNetCleanTemplate.Domain.ValueObjects.Role;

namespace DomainTests
{
    public class RoleTests
    {
        [Theory]
        [InlineData("")]
        [InlineData(" ")]
        public void Create_ShouldThrow_WhenRoleIsInvalid(string value)
        {
            Assert.Throws<ArgumentException>(() => new Role(new RoleName(value)));
        }

        [Fact]
        public void Create_ShouldSucceed_WhenRoleIsValid()
        {
            var role = new Role(new RoleName("Admin"));
            Assert.Equal("Admin", role.Name.Value);
        }

        [Fact]
        public void Role_Created_With_Valid_Properties()
        {
            var name = new RoleName("Admin");
            var role = new Role(name);
            Assert.Equal(name, role.Name);
            Assert.NotEqual(Guid.Empty, role.Id);
        }

        [Fact]
        public void Role_Created_With_Invalid_Name_Throws()
        {
            Assert.Throws<ArgumentException>(() => new Role(new RoleName("")));
        }

        [Fact]
        public void UserRoles_ReturnsReadOnlyCollection()
        {
            var role = new Role(new RoleName("Admin"));
            var userRoles = role.UserRoles;
            Assert.Empty(userRoles);
            Assert.IsType<IReadOnlyCollection<UserRole>>(userRoles, false);
        }

        [Fact]
        public void ProtectedConstructor_ForEFCore_DoesNotThrow()
        {
            var ctor = typeof(Role).GetConstructor(
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance,
                null,
                Type.EmptyTypes,
                null
            );
            var role = ctor!.Invoke(null);
            Assert.NotNull(role);
        }
    }
}

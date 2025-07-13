using DotNetCleanTemplate.Domain.Entities;
using DotNetCleanTemplate.Domain.ValueObjects.Role;

namespace DomainTests
{
    public class RoleTests
    {
        [Fact]
        public void Role_Created_With_Valid_Properties()
        {
            var name = new RoleName("Admin");
            var role = new Role(name);
            Assert.Equal(name, role.Name);
            Assert.NotEqual(Guid.Empty, role.Id);
        }
    }
}

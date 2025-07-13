using DotNetCleanTemplate.Domain.Entities;
using DotNetCleanTemplate.Domain.ValueObjects.User;

namespace DomainTests
{
    public class UserTests
    {
        [Fact]
        public void User_Created_With_Valid_Properties()
        {
            var name = new UserName("user1");
            var email = new Email("user1@example.com");
            var hash = new PasswordHash("12345678901234567890");
            var user = new User(name, email, hash);
            Assert.Equal(name, user.Name);
            Assert.Equal(email, user.Email);
            Assert.Equal(hash, user.PasswordHash);
            // Проверка UserRoles по умолчанию
            Assert.Empty(user.UserRoles);
        }

        [Fact]
        public void UserRoles_CanBeAdded()
        {
            var user = new User(
                new UserName("user"),
                new Email("a@b.cd"),
                new PasswordHash("12345678901234567890")
            );
            var role = new Role(new("admin"));
            user.AssignRole(role);
            Assert.Single(user.UserRoles);
            Assert.Equal(user.Id, user.UserRoles.First().UserId);
            Assert.Equal(role.Id, user.UserRoles.First().RoleId);
        }
    }
}

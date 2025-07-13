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
            Assert.Empty(user.RoleIds);
        }

        [Fact]
        public void User_AddRole_AddsRoleId()
        {
            var user = new User(
                new UserName("user"),
                new Email("a@b.cd"),
                new PasswordHash("12345678901234567890")
            );
            var roleId = Guid.NewGuid();
            user.AddRole(roleId);
            Assert.Contains(roleId, user.RoleIds);
        }

        [Fact]
        public void User_AddRole_Duplicate_NotAddedTwice()
        {
            var user = new User(
                new UserName("user"),
                new Email("a@b.cd"),
                new PasswordHash("12345678901234567890")
            );
            var roleId = Guid.NewGuid();
            user.AddRole(roleId);
            user.AddRole(roleId);
            Assert.Equal(1, user.RoleIds.Count(r => r == roleId));
        }

        [Fact]
        public void User_RemoveRole_RemovesRoleId()
        {
            var user = new User(
                new UserName("user"),
                new Email("a@b.cd"),
                new PasswordHash("12345678901234567890")
            );
            var roleId = Guid.NewGuid();
            user.AddRole(roleId);
            user.RemoveRole(roleId);
            Assert.DoesNotContain(roleId, user.RoleIds);
        }
    }
}

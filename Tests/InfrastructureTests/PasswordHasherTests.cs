using DotNetCleanTemplate.Infrastructure.Services;
using Xunit;

namespace InfrastructureTests
{
    public class PasswordHasherTests
    {
        private readonly PasswordHasher _hasher = new();

        [Fact]
        public void HashPassword_And_VerifyPassword_Works()
        {
            var password = "TestPassword123!";
            var hash = _hasher.HashPassword(password);
            Assert.True(_hasher.VerifyPassword(hash, password));
        }

        [Fact]
        public void VerifyPassword_Fails_On_WrongPassword()
        {
            var password = "TestPassword123!";
            var hash = _hasher.HashPassword(password);
            Assert.False(_hasher.VerifyPassword(hash, "WrongPassword"));
        }

        [Fact]
        public void HashPassword_Produces_Different_Hash_For_Different_Passwords()
        {
            var hash1 = _hasher.HashPassword("password1");
            var hash2 = _hasher.HashPassword("password2");
            Assert.NotEqual(hash1, hash2);
        }

        [Fact]
        public void HashPassword_Produces_Different_Hash_For_Same_Password()
        {
            var password = "SamePassword";
            var hash1 = _hasher.HashPassword(password);
            var hash2 = _hasher.HashPassword(password);
            Assert.NotEqual(hash1, hash2); // из-за соли
        }
    }
}

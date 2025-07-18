using DotNetCleanTemplate.Infrastructure.Services;

namespace DotNetCleanTemplate.UnitTests.Infrastructure
{
    public class PasswordHasherTests
    {
        private readonly PasswordHasher _hasher = new();

        [Theory]
        [InlineData("")]
        [InlineData(null)]
        public void HashPassword_EmptyOrNull_Throws(string? password)
        {
            Assert.ThrowsAny<Exception>(() => _hasher.HashPassword(password!));
        }

        [Theory]
        [InlineData(null, "password")]
        [InlineData("", "password")]
        [InlineData("hash", null)]
        [InlineData("hash", "")]
        public void VerifyPassword_EmptyOrNull_ReturnsFalse(string? hash, string? password)
        {
            _hasher.VerifyPassword(hash!, password!).ShouldBeFalse();
        }

        [Fact]
        public void VerifyPassword_InvalidHashFormat_ReturnsFalse()
        {
            _hasher.VerifyPassword("not.a.valid.hash", "password").ShouldBeFalse();
            _hasher.VerifyPassword("justonepart", "password").ShouldBeFalse();
        }

        [Fact]
        public void VerifyPassword_WrongPassword_ReturnsFalse()
        {
            var hash = _hasher.HashPassword("correct");
            _hasher.VerifyPassword(hash, "wrong").ShouldBeFalse();
        }

        [Fact]
        public void VerifyPassword_CorrectPassword_ReturnsTrue()
        {
            var hash = _hasher.HashPassword("secret");
            _hasher.VerifyPassword(hash, "secret").ShouldBeTrue();
        }

        [Fact]
        public void HashPassword_SamePassword_DifferentHashes()
        {
            var hash1 = _hasher.HashPassword("repeat");
            var hash2 = _hasher.HashPassword("repeat");
            Assert.NotEqual(hash1, hash2);
        }
    }

    public static class PasswordHasherTestExtensions
    {
        public static void ShouldBeTrue(this bool value) => Assert.True(value);

        public static void ShouldBeFalse(this bool value) => Assert.False(value);
    }
}

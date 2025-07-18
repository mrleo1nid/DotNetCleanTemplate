using DotNetCleanTemplate.Domain.ValueObjects.User;

namespace DotNetCleanTemplate.UnitTests.Domain
{
    public class PasswordHashTests
    {
        [Theory]
        [InlineData("12345678901234567890")]
        [InlineData("hashhashhashhashhash")]
        public void PasswordHash_Valid_Accepted(string value)
        {
            var hash = new PasswordHash(value);
            Assert.Equal(value, hash.Value);
        }

        [Theory]
        [InlineData("")]
        [InlineData(" ")]
        public void PasswordHash_Empty_Throws(string value)
        {
            Assert.Throws<ArgumentException>(() => new PasswordHash(value));
        }

        [Fact]
        public void PasswordHash_TooShort_Throws()
        {
            var shortHash = new string('a', 5);
            Assert.Throws<ArgumentException>(() => new PasswordHash(shortHash));
        }

        [Fact]
        public void PasswordHash_ToString_ReturnsValue()
        {
            var hash = new PasswordHash("12345678901234567890");
            Assert.Equal("12345678901234567890", hash.ToString());
        }

        [Fact]
        public void PasswordHash_Equality()
        {
            var a = new PasswordHash("hashhashhashhashhash");
            var b = new PasswordHash("hashhashhashhashhash");
            Assert.Equal(a, b);
        }
    }
}

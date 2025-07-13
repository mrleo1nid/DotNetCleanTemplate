using DotNetCleanTemplate.Domain.ValueObjects.User;

namespace DomainTests
{
    public class EmailTests
    {
        [Theory]
        [InlineData("test@example.com")]
        [InlineData("user.name@domain.co")]
        [InlineData("a@b.cd")]
        public void Email_Valid_Emails_AreAccepted(string value)
        {
            var email = new Email(value);
            Assert.Equal(value, email.Value);
        }

        [Theory]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData("notanemail")]
        [InlineData("@domain.com")]
        [InlineData("user@.com")]
        [InlineData("user@domain")]
        [InlineData("user@domain..com")]
        public void Email_Invalid_Throws(string value)
        {
            Assert.Throws<ArgumentException>(() => new Email(value));
        }

        [Fact]
        public void Email_TooShort_Throws()
        {
            Assert.Throws<ArgumentException>(() => new Email("a@b"));
        }

        [Fact]
        public void Email_TooLong_Throws()
        {
            var longEmail = new string('a', 250) + "@example.com";
            Assert.Throws<ArgumentException>(() => new Email(longEmail));
        }

        [Fact]
        public void Email_ToString_ReturnsValue()
        {
            var email = new Email("test@example.com");
            Assert.Equal("test@example.com", email.ToString());
        }

        [Fact]
        public void Email_Equality_IsCaseInsensitive()
        {
            var a = new Email("Test@Example.com");
            var b = new Email("test@example.com");
            Assert.Equal(a, b);
        }
    }
}

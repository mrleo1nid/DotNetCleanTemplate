using DotNetCleanTemplate.Domain.ValueObjects.User;

namespace DomainTests
{
    public class EmailTests
    {
        [Theory]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData("notanemail")]
        [InlineData("@domain.com")]
        [InlineData("user@.com")]
        [InlineData("user@domain")]
        [InlineData("user@domain..com")]
        public void Create_ShouldThrow_WhenEmailIsInvalid(string value)
        {
            Assert.Throws<ArgumentException>(() => new Email(value));
        }

        [Theory]
        [InlineData("test@example.com")]
        [InlineData("user.name@domain.co")]
        [InlineData("a@b.cd")]
        public void Create_ShouldSucceed_WhenEmailIsValid(string value)
        {
            var email = new Email(value);
            Assert.Equal(value, email.Value);
        }

        [Fact]
        public void ToString_ReturnsValue()
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

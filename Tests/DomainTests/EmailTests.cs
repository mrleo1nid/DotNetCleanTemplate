using DotNetCleanTemplate.Domain.Common;
using DotNetCleanTemplate.Domain.ValueObjects.User;

namespace DomainTests
{
    public class EmailTests
    {
        [Theory]
        [InlineData("user@example.com")]
        [InlineData("user.name+tag@sub.domain.com")]
        [InlineData("u@a.co")]
        public void ValidEmails_CreatesSuccessfully(string email)
        {
            var obj = new Email(email);
            Assert.Equal(email, obj.Value);
        }

        [Theory]
        [InlineData("")]
        [InlineData(null)]
        [InlineData("userexample.com")]
        [InlineData("user@.com")]
        [InlineData("user@com")]
        [InlineData("user@com.")]
        [InlineData("user@com..com")]
        [InlineData("user@com com")]
        [InlineData("user@@example.com")]
        [InlineData("user..name@example.com")]
        public void InvalidEmails_ThrowsArgumentException(string? email)
        {
            Assert.Throws<ArgumentException>(() => new Email(email!));
        }

        [Fact]
        public void TooShortEmail_Throws()
        {
            // Email короче минимальной длины
            var shortLocal = new string('a', DomainConstants.MinEmailLength - 3);
            var shortEmail = $"{shortLocal}@a";
            Assert.True(shortEmail.Length < DomainConstants.MinEmailLength);
            Assert.Throws<ArgumentException>(() => new Email(shortEmail));
        }

        [Fact]
        public void TooLongEmail_Throws()
        {
            // Email длиннее максимальной длины
            var longLocal = new string('a', DomainConstants.MaxEmailLength);
            var longEmail = $"{longLocal}@example.com";
            Assert.True(longEmail.Length > DomainConstants.MaxEmailLength);
            Assert.Throws<ArgumentException>(() => new Email(longEmail));
        }

        [Fact]
        public void Email_Equality_IsCaseInsensitive()
        {
            var e1 = new Email("User@Example.com");
            var e2 = new Email("user@example.com");
            Assert.Equal(e1, e2);
            Assert.True(e1.Equals(e2));
            Assert.Equal(e1.GetHashCode(), e2.GetHashCode());
        }

        [Fact]
        public void Email_ToString_ReturnsValue()
        {
            var email = "user@example.com";
            var obj = new Email(email);
            Assert.Equal(email, obj.ToString());
        }
    }
}

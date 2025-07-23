using DotNetCleanTemplate.Domain.ValueObjects.User;

namespace DotNetCleanTemplate.UnitTests.Domain
{
    public class UserNameTests
    {
        [Theory]
        [InlineData("user1")]
        [InlineData("UserName")]
        [InlineData("abc")]
        public void UserName_Valid_Accepted(string value)
        {
            var name = new UserName(value);
            Assert.Equal(value.Trim(), name.Value);
        }

        [Theory]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData("ab")]
        public void UserName_Invalid_Throws(string value)
        {
            Assert.Throws<ArgumentException>(() => new UserName(value));
        }

        [Fact]
        public void UserName_TooLong_Throws()
        {
            var longName = new string('a', 300);
            Assert.Throws<ArgumentException>(() => new UserName(longName));
        }

        [Fact]
        public void UserName_ToString_ReturnsValue()
        {
            var name = new UserName("user1");
            Assert.Equal("user1", name.ToString());
        }

        [Fact]
        public void UserName_Equality_IsCaseInsensitive()
        {
            var a = new UserName("User");
            var b = new UserName("user");
            Assert.Equal(a, b);
        }
    }
}

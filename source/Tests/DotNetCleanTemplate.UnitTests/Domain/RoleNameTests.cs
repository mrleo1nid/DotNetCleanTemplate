using DotNetCleanTemplate.Domain.ValueObjects.Role;

namespace DotNetCleanTemplate.UnitTests.Domain
{
    public class RoleNameTests
    {
        [Theory]
        [InlineData("Admin")]
        [InlineData("User")]
        [InlineData("abc")]
        public void RoleName_Valid_Accepted(string value)
        {
            var name = new RoleName(value);
            Assert.Equal(value.Trim(), name.Value);
        }

        [Theory]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData("ab")]
        public void RoleName_Invalid_Throws(string value)
        {
            Assert.Throws<ArgumentException>(() => new RoleName(value));
        }

        [Fact]
        public void RoleName_TooLong_Throws()
        {
            var longName = new string('a', 300);
            Assert.Throws<ArgumentException>(() => new RoleName(longName));
        }

        [Fact]
        public void RoleName_ToString_ReturnsValue()
        {
            var name = new RoleName("Admin");
            Assert.Equal("Admin", name.ToString());
        }

        [Fact]
        public void RoleName_Equality_IsCaseInsensitive()
        {
            var a = new RoleName("Admin");
            var b = new RoleName("admin");
            Assert.Equal(a, b);
        }
    }
}

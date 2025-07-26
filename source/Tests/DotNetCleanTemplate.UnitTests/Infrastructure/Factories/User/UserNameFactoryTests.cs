using DotNetCleanTemplate.Domain.Factories.User;
using DotNetCleanTemplate.Domain.ValueObjects.User;
using DotNetCleanTemplate.Infrastructure.Factories.User;
using FluentAssertions;
using Xunit;

namespace DotNetCleanTemplate.UnitTests.Infrastructure.Factories.User
{
    public class UserNameFactoryTests
    {
        private readonly IUserNameFactory _factory;

        public UserNameFactoryTests()
        {
            _factory = new UserNameFactory();
        }

        [Fact]
        public void Create_WithValidUserName_ShouldReturnUserName()
        {
            // Arrange
            var validUserName = "TestUser123";

            // Act
            var result = _factory.Create(validUserName);

            // Assert
            result.Should().NotBeNull();
            result.Should().BeOfType<UserName>();
            result.Value.Should().Be(validUserName);
        }

        [Fact]
        public void Create_WithEmptyString_ShouldThrowArgumentException()
        {
            // Arrange
            var emptyUserName = "";

            // Act & Assert
            var action = () => _factory.Create(emptyUserName);
            action.Should().Throw<ArgumentException>();
        }

        [Fact]
        public void Create_WithTooShortUserName_ShouldThrowArgumentException()
        {
            // Arrange
            var shortUserName = "ab";

            // Act & Assert
            var action = () => _factory.Create(shortUserName);
            action.Should().Throw<ArgumentException>();
        }
    }
}

using DotNetCleanTemplate.Domain.Factories.User;
using DotNetCleanTemplate.Domain.ValueObjects.User;
using DotNetCleanTemplate.Infrastructure.Factories.User;
using FluentAssertions;
using Xunit;

namespace DotNetCleanTemplate.UnitTests.Infrastructure.Factories.User
{
    public class EmailFactoryTests
    {
        private readonly IEmailFactory _factory;

        public EmailFactoryTests()
        {
            _factory = new EmailFactory();
        }

        [Fact]
        public void Create_WithValidEmail_ShouldReturnEmail()
        {
            // Arrange
            var validEmail = "test@example.com";

            // Act
            var result = _factory.Create(validEmail);

            // Assert
            result.Should().NotBeNull();
            result.Should().BeOfType<Email>();
            result.Value.Should().Be(validEmail);
        }

        [Fact]
        public void Create_WithEmptyString_ShouldThrowArgumentException()
        {
            // Arrange
            var emptyEmail = "";

            // Act & Assert
            var action = () => _factory.Create(emptyEmail);
            action.Should().Throw<ArgumentException>();
        }

        [Fact]
        public void Create_WithInvalidEmail_ShouldThrowArgumentException()
        {
            // Arrange
            var invalidEmail = "invalid-email";

            // Act & Assert
            var action = () => _factory.Create(invalidEmail);
            action.Should().Throw<ArgumentException>();
        }
    }
}

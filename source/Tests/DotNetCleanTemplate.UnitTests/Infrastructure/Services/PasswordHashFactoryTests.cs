using DotNetCleanTemplate.Domain.Factories.User;
using DotNetCleanTemplate.Domain.ValueObjects.User;
using DotNetCleanTemplate.Infrastructure.Factories.User;
using FluentAssertions;
using Xunit;

namespace DotNetCleanTemplate.UnitTests.Infrastructure.Services
{
    public class PasswordHashFactoryTests
    {
        private readonly IPasswordHashFactory _factory;

        public PasswordHashFactoryTests()
        {
            _factory = new PasswordHashFactory();
        }

        [Fact]
        public void Create_WithValidHash_ShouldReturnPasswordHash()
        {
            // Arrange
            var validHash = "valid_password_hash_123";

            // Act
            var result = _factory.Create(validHash);

            // Assert
            result.Should().NotBeNull();
            result.Should().BeOfType<PasswordHash>();
            result.Value.Should().Be(validHash);
        }

        [Fact]
        public void Create_WithEmptyString_ShouldThrowArgumentException()
        {
            // Arrange
            var emptyHash = "";

            // Act & Assert
            var action = () => _factory.Create(emptyHash);
            action
                .Should()
                .Throw<ArgumentException>()
                .WithMessage("Password hash cannot be empty.*");
        }

        [Fact]
        public void Create_WithWhitespaceString_ShouldThrowArgumentException()
        {
            // Arrange
            var whitespaceHash = "   ";

            // Act & Assert
            var action = () => _factory.Create(whitespaceHash);
            action
                .Should()
                .Throw<ArgumentException>()
                .WithMessage("Password hash cannot be empty.*");
        }

        [Fact]
        public void Create_WithNullString_ShouldThrowArgumentException()
        {
            // Arrange
            string? nullHash = null;

            // Act & Assert
            var action = () => _factory.Create(nullHash!);
            action
                .Should()
                .Throw<ArgumentException>()
                .WithMessage("Password hash cannot be empty.*");
        }
    }
}

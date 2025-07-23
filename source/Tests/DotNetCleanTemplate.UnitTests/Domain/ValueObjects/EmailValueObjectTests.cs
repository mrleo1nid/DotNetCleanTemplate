using DotNetCleanTemplate.Domain.ValueObjects.User;
using Xunit;

namespace DotNetCleanTemplate.UnitTests.Domain.ValueObjects;

public class EmailValueObjectTests
{
    [Fact]
    public void Constructor_WithValidEmail_ShouldCreateInstance()
    {
        // Arrange & Act
        var email = new Email("test@example.com");

        // Assert
        Assert.Equal("test@example.com", email.Value);
    }

    [Fact]
    public void Constructor_WithInvalidEmail_ShouldThrowException()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => new Email("invalid-email"));
    }

    [Fact]
    public void Constructor_WithEmptyEmail_ShouldThrowException()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => new Email(""));
    }

    [Fact]
    public void Constructor_WithNullEmail_ShouldThrowException()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => new Email(null!));
    }

    [Fact]
    public void Constructor_WithTooLongEmail_ShouldThrowException()
    {
        // Arrange
        var longEmail = new string('a', 256) + "@example.com";

        // Act & Assert
        Assert.Throws<ArgumentException>(() => new Email(longEmail));
    }

    [Fact]
    public void ToString_ShouldReturnEmailValue()
    {
        // Arrange
        var email = new Email("test@example.com");

        // Act
        var result = email.ToString();

        // Assert
        Assert.Equal("test@example.com", result);
    }

    [Fact]
    public void Equals_WithSameEmail_ShouldReturnTrue()
    {
        // Arrange
        var email1 = new Email("test@example.com");
        var email2 = new Email("test@example.com");

        // Act & Assert
        Assert.Equal(email1, email2);
    }

    [Fact]
    public void Equals_WithDifferentEmail_ShouldReturnFalse()
    {
        // Arrange
        var email1 = new Email("test1@example.com");
        var email2 = new Email("test2@example.com");

        // Act & Assert
        Assert.NotEqual(email1, email2);
    }

    [Fact]
    public void GetHashCode_ShouldReturnConsistentHash()
    {
        // Arrange
        var email1 = new Email("test@example.com");
        var email2 = new Email("test@example.com");

        // Act
        var hash1 = email1.GetHashCode();
        var hash2 = email2.GetHashCode();

        // Assert
        Assert.Equal(hash1, hash2);
    }
}

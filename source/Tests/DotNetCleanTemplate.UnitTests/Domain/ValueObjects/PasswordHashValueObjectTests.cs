using DotNetCleanTemplate.Domain.ValueObjects.User;

namespace DotNetCleanTemplate.UnitTests.Domain.ValueObjects;

public class PasswordHashValueObjectTests
{
    [Fact]
    public void Constructor_WithValidHash_ShouldCreateInstance()
    {
        // Arrange & Act
        var hash = new PasswordHash("valid.hash.value");

        // Assert
        Assert.Equal("valid.hash.value", hash.Value);
    }

    [Fact]
    public void Constructor_WithEmptyHash_ShouldThrowException()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => new PasswordHash(""));
    }

    [Fact]
    public void Constructor_WithNullHash_ShouldThrowException()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => new PasswordHash(null!));
    }

    [Fact]
    public void ToString_ShouldReturnHashValue()
    {
        // Arrange
        var hash = new PasswordHash("valid.hash.value");

        // Act
        var result = hash.ToString();

        // Assert
        Assert.Equal("valid.hash.value", result);
    }

    [Fact]
    public void Equals_WithSameHash_ShouldReturnTrue()
    {
        // Arrange
        var hash1 = new PasswordHash("valid.hash.value");
        var hash2 = new PasswordHash("valid.hash.value");

        // Act & Assert
        Assert.Equal(hash1, hash2);
    }

    [Fact]
    public void Equals_WithDifferentHash_ShouldReturnFalse()
    {
        // Arrange
        var hash1 = new PasswordHash("hash1.value");
        var hash2 = new PasswordHash("hash2.value");

        // Act & Assert
        Assert.NotEqual(hash1, hash2);
    }

    [Fact]
    public void GetHashCode_ShouldReturnConsistentHash()
    {
        // Arrange
        var hash1 = new PasswordHash("valid.hash.value");
        var hash2 = new PasswordHash("valid.hash.value");

        // Act
        var hashCode1 = hash1.GetHashCode();
        var hashCode2 = hash2.GetHashCode();

        // Assert
        Assert.Equal(hashCode1, hashCode2);
    }
}

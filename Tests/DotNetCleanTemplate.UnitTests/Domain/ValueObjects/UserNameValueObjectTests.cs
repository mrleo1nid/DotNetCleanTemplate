using DotNetCleanTemplate.Domain.ValueObjects.User;
using Xunit;

namespace DotNetCleanTemplate.UnitTests.Domain.ValueObjects;

public class UserNameValueObjectTests
{
    [Fact]
    public void Constructor_WithValidName_ShouldCreateInstance()
    {
        // Arrange & Act
        var userName = new UserName("testuser");

        // Assert
        Assert.Equal("testuser", userName.Value);
    }

    [Fact]
    public void Constructor_WithEmptyName_ShouldThrowException()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => new UserName(""));
    }

    [Fact]
    public void Constructor_WithNullName_ShouldThrowException()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => new UserName(null!));
    }

    [Fact]
    public void Constructor_WithTooShortName_ShouldThrowException()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => new UserName("ab"));
    }

    [Fact]
    public void Constructor_WithTooLongName_ShouldThrowException()
    {
        // Arrange
        var longName = new string('a', 256);

        // Act & Assert
        Assert.Throws<ArgumentException>(() => new UserName(longName));
    }

    [Fact]
    public void ToString_ShouldReturnNameValue()
    {
        // Arrange
        var userName = new UserName("testuser");

        // Act
        var result = userName.ToString();

        // Assert
        Assert.Equal("testuser", result);
    }

    [Fact]
    public void Equals_WithSameName_ShouldReturnTrue()
    {
        // Arrange
        var userName1 = new UserName("testuser");
        var userName2 = new UserName("testuser");

        // Act & Assert
        Assert.Equal(userName1, userName2);
    }

    [Fact]
    public void Equals_WithDifferentName_ShouldReturnFalse()
    {
        // Arrange
        var userName1 = new UserName("user1");
        var userName2 = new UserName("user2");

        // Act & Assert
        Assert.NotEqual(userName1, userName2);
    }

    [Fact]
    public void GetHashCode_ShouldReturnConsistentHash()
    {
        // Arrange
        var userName1 = new UserName("testuser");
        var userName2 = new UserName("testuser");

        // Act
        var hashCode1 = userName1.GetHashCode();
        var hashCode2 = userName2.GetHashCode();

        // Assert
        Assert.Equal(hashCode1, hashCode2);
    }
}

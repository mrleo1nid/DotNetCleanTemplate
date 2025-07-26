using DotNetCleanTemplate.Domain.ValueObjects.Role;

namespace DotNetCleanTemplate.UnitTests.Domain.ValueObjects;

public class RoleNameValueObjectTests
{
    [Fact]
    public void Constructor_WithValidName_ShouldCreateInstance()
    {
        // Arrange & Act
        var roleName = new RoleName("admin");

        // Assert
        Assert.Equal("admin", roleName.Value);
    }

    [Fact]
    public void Constructor_WithEmptyName_ShouldThrowException()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => new RoleName(""));
    }

    [Fact]
    public void Constructor_WithNullName_ShouldThrowException()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => new RoleName(null!));
    }

    [Fact]
    public void Constructor_WithTooShortName_ShouldThrowException()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => new RoleName("ab"));
    }

    [Fact]
    public void Constructor_WithTooLongName_ShouldThrowException()
    {
        // Arrange
        var longName = new string('a', 256);

        // Act & Assert
        Assert.Throws<ArgumentException>(() => new RoleName(longName));
    }

    [Fact]
    public void ToString_ShouldReturnNameValue()
    {
        // Arrange
        var roleName = new RoleName("admin");

        // Act
        var result = roleName.ToString();

        // Assert
        Assert.Equal("admin", result);
    }

    [Fact]
    public void Equals_WithSameName_ShouldReturnTrue()
    {
        // Arrange
        var roleName1 = new RoleName("admin");
        var roleName2 = new RoleName("admin");

        // Act & Assert
        Assert.Equal(roleName1, roleName2);
    }

    [Fact]
    public void Equals_WithDifferentName_ShouldReturnFalse()
    {
        // Arrange
        var roleName1 = new RoleName("admin");
        var roleName2 = new RoleName("user");

        // Act & Assert
        Assert.NotEqual(roleName1, roleName2);
    }

    [Fact]
    public void GetHashCode_ShouldReturnConsistentHash()
    {
        // Arrange
        var roleName1 = new RoleName("admin");
        var roleName2 = new RoleName("admin");

        // Act
        var hashCode1 = roleName1.GetHashCode();
        var hashCode2 = roleName2.GetHashCode();

        // Assert
        Assert.Equal(hashCode1, hashCode2);
    }
}

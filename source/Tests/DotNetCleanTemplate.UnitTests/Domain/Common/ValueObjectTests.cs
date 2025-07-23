using DotNetCleanTemplate.Domain.Common;
using Xunit;

namespace DotNetCleanTemplate.UnitTests.Domain.Common;

public class TestValueObject : ValueObject
{
    public string Property1 { get; }
    public int Property2 { get; }

    public TestValueObject(string property1, int property2)
    {
        Property1 = property1;
        Property2 = property2;
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Property1;
        yield return Property2;
    }
}

public class ValueObjectTests
{
    [Fact]
    public void Equals_WithSameComponents_ShouldReturnTrue()
    {
        // Arrange
        var valueObject1 = new TestValueObject("test", 123);
        var valueObject2 = new TestValueObject("test", 123);

        // Act & Assert
        Assert.Equal(valueObject1, valueObject2);
    }

    [Fact]
    public void Equals_WithDifferentComponents_ShouldReturnFalse()
    {
        // Arrange
        var valueObject1 = new TestValueObject("test1", 123);
        var valueObject2 = new TestValueObject("test2", 123);

        // Act & Assert
        Assert.NotEqual(valueObject1, valueObject2);
    }

    [Fact]
    public void Equals_WithNull_ShouldReturnFalse()
    {
        // Arrange
        var valueObject = new TestValueObject("test", 123);

        // Act & Assert
        Assert.False(valueObject.Equals(null));
    }

    [Fact]
    public void GetHashCode_ShouldReturnConsistentHash()
    {
        // Arrange
        var valueObject1 = new TestValueObject("test", 123);
        var valueObject2 = new TestValueObject("test", 123);

        // Act
        var hashCode1 = valueObject1.GetHashCode();
        var hashCode2 = valueObject2.GetHashCode();

        // Assert
        Assert.Equal(hashCode1, hashCode2);
    }

    [Fact]
    public void GetHashCode_WithNullComponents_ShouldHandleGracefully()
    {
        // Arrange
        var valueObject = new TestValueObjectWithNull("test", null);

        // Act
        var hashCode = valueObject.GetHashCode();

        // Assert
        Assert.NotEqual(0, hashCode);
    }
}

public class TestValueObjectWithNull : ValueObject
{
    public string Property1 { get; }
    public string? Property2 { get; }

    public TestValueObjectWithNull(string property1, string? property2)
    {
        Property1 = property1;
        Property2 = property2;
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Property1;
        if (Property2 != null)
            yield return Property2;
    }
}

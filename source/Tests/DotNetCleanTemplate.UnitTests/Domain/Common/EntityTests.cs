using DotNetCleanTemplate.Domain.Common;
using Xunit;

namespace DotNetCleanTemplate.UnitTests.Domain.Common;

public class TestEntity : Entity<Guid>
{
    public TestEntity() { }

    public TestEntity(Guid id)
    {
        Id = id;
    }
}

public class EntityTests
{
    [Fact]
    public void Constructor_ShouldInitializeId()
    {
        // Act
        var entity = new TestEntity();

        // Assert
        Assert.Equal(Guid.Empty, entity.Id);
    }

    [Fact]
    public void Constructor_WithId_ShouldSetId()
    {
        // Arrange
        var id = Guid.NewGuid();

        // Act
        var entity = new TestEntity(id);

        // Assert
        Assert.Equal(id, entity.Id);
    }

    [Fact]
    public void GetHashCode_ShouldReturnIdHashCode()
    {
        // Arrange
        var entity = new TestEntity();

        // Act
        var hashCode = entity.GetHashCode();

        // Assert
        Assert.Equal(entity.Id.GetHashCode(), hashCode);
    }

    [Fact]
    public void Equals_WithSameId_ShouldReturnTrue()
    {
        // Arrange
        var id = Guid.NewGuid();
        var entity1 = new TestEntity(id);
        var entity2 = new TestEntity(id);

        // Act & Assert
        Assert.Equal(entity1, entity2);
    }

    [Fact]
    public void Equals_WithDifferentId_ShouldReturnFalse()
    {
        // Arrange
        var entity1 = new TestEntity(Guid.NewGuid());
        var entity2 = new TestEntity(Guid.NewGuid());

        // Act & Assert
        Assert.NotEqual(entity1, entity2);
    }

    [Fact]
    public void Equals_WithNull_ShouldReturnFalse()
    {
        // Arrange
        var entity = new TestEntity();

        // Act & Assert
        Assert.False(entity.Equals(null));
    }

    [Fact]
    public void Equals_WithDifferentType_ShouldReturnFalse()
    {
        // Arrange
        var entity = new TestEntity();
        var otherObject = new object();

        // Act & Assert
        Assert.False(entity.Equals(otherObject));
    }
}

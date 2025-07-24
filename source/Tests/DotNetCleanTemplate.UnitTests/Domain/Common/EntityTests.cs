using DotNetCleanTemplate.Domain.Common;
using Xunit;

namespace DotNetCleanTemplate.UnitTests.Domain.Common;

public class EntityTests
{
    #region Test Entity Implementation

    private class TestEntity : Entity<int>
    {
        public string Name { get; set; }

        public TestEntity(int id, string name)
        {
            Id = id;
            Name = name;
        }

        public void UpdateName(string newName)
        {
            Name = newName;
            SetUpdated();
        }
    }

    #endregion

    #region Constructor Tests

    [Fact]
    public void Entity_WhenCreated_ShouldSetCreatedAt()
    {
        // Arrange & Act
        var entity = new TestEntity(1, "Test");

        // Assert
        Assert.NotEqual(default, entity.CreatedAt);
        Assert.True(entity.CreatedAt <= DateTime.UtcNow);
        Assert.True(entity.CreatedAt > DateTime.UtcNow.AddMinutes(-1));
    }

    [Fact]
    public void Entity_WhenCreated_ShouldSetUpdatedAt()
    {
        // Arrange & Act
        var entity = new TestEntity(1, "Test");

        // Assert
        Assert.NotEqual(default, entity.UpdatedAt);
        Assert.True(entity.UpdatedAt <= DateTime.UtcNow);
        Assert.True(entity.UpdatedAt > DateTime.UtcNow.AddMinutes(-1));
    }

    #endregion

    #region Equals Tests

    [Fact]
    public void Equals_WithSameId_ShouldReturnTrue()
    {
        // Arrange
        var entity1 = new TestEntity(1, "Test1");
        var entity2 = new TestEntity(1, "Test2");

        // Act
        var result = entity1.Equals(entity2);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void Equals_WithDifferentId_ShouldReturnFalse()
    {
        // Arrange
        var entity1 = new TestEntity(1, "Test");
        var entity2 = new TestEntity(2, "Test");

        // Act
        var result = entity1.Equals(entity2);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void Equals_WithNull_ShouldReturnFalse()
    {
        // Arrange
        var entity = new TestEntity(1, "Test");

        // Act
        var result = entity.Equals((TestEntity?)null);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void Equals_WithSameReference_ShouldReturnTrue()
    {
        // Arrange
        var entity = new TestEntity(1, "Test");

        // Act
        var result = entity.Equals(entity);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void Equals_WithObjectParameter_WithSameId_ShouldReturnTrue()
    {
        // Arrange
        var entity1 = new TestEntity(1, "Test1");
        var entity2 = new TestEntity(1, "Test2");

        // Act
        var result = entity1.Equals((object)entity2);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void Equals_WithObjectParameter_WithDifferentId_ShouldReturnFalse()
    {
        // Arrange
        var entity1 = new TestEntity(1, "Test");
        var entity2 = new TestEntity(2, "Test");

        // Act
        var result = entity1.Equals((object)entity2);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void Equals_WithObjectParameter_WithNull_ShouldReturnFalse()
    {
        // Arrange
        var entity = new TestEntity(1, "Test");

        // Act
        var result = entity.Equals((object?)null);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void Equals_WithObjectParameter_WithDifferentType_ShouldReturnFalse()
    {
        // Arrange
        var entity = new TestEntity(1, "Test");
        var differentObject = "Not an entity";

        // Act
        var result = entity.Equals(differentObject);

        // Assert
        Assert.False(result);
    }

    #endregion

    #region GetHashCode Tests

    [Fact]
    public void GetHashCode_WithSameId_ShouldReturnSameHashCode()
    {
        // Arrange
        var entity1 = new TestEntity(1, "Test1");
        var entity2 = new TestEntity(1, "Test2");

        // Act
        var hash1 = entity1.GetHashCode();
        var hash2 = entity2.GetHashCode();

        // Assert
        Assert.Equal(hash1, hash2);
    }

    [Fact]
    public void GetHashCode_WithDifferentId_ShouldReturnDifferentHashCode()
    {
        // Arrange
        var entity1 = new TestEntity(1, "Test");
        var entity2 = new TestEntity(2, "Test");

        // Act
        var hash1 = entity1.GetHashCode();
        var hash2 = entity2.GetHashCode();

        // Assert
        Assert.NotEqual(hash1, hash2);
    }

    [Fact]
    public void GetHashCode_WithNullId_ShouldReturnZero()
    {
        // Arrange
        var entity = new TestEntityWithNullableId(null, "Test");

        // Act
        var hash = entity.GetHashCode();

        // Assert
        Assert.Equal(0, hash);
    }

    #endregion

    #region SetUpdated Tests

    [Fact]
    public void SetUpdated_WhenCalled_ShouldUpdateUpdatedAt()
    {
        // Arrange
        var entity = new TestEntity(1, "Test");
        var originalUpdatedAt = entity.UpdatedAt;

        // Act
        entity.UpdateName("Updated");

        // Assert
        Assert.True(entity.UpdatedAt >= originalUpdatedAt);
        Assert.True(entity.UpdatedAt <= DateTime.UtcNow);
    }

    #endregion

    #region Helper Classes

    private class TestEntityWithNullableId : Entity<int?>
    {
        public string Name { get; set; }

        public TestEntityWithNullableId(int? id, string name)
        {
            Id = id;
            Name = name;
        }
    }

    #endregion
}

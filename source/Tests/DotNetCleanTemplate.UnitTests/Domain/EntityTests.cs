using DotNetCleanTemplate.Domain.Common;

namespace DotNetCleanTemplate.UnitTests.Domain
{
    public class EntityTests
    {
        private class DummyEntity : Entity<Guid>
        {
            public DummyEntity(Guid id)
            {
                Id = id;
            }
        }

        [Fact]
        public void EntitiesWithSameId_ShouldBeEqual()
        {
            var id = Guid.NewGuid();
            var a = new DummyEntity(id);
            var b = new DummyEntity(id);
            Assert.Equal(a, b);
        }

        [Fact]
        public void EntitiesWithDifferentId_ShouldNotBeEqual()
        {
            var a = new DummyEntity(Guid.NewGuid());
            var b = new DummyEntity(Guid.NewGuid());
            Assert.NotEqual(a, b);
        }

        [Fact]
        public void Entity_SetUpdated_ChangesUpdatedAt()
        {
            var entity = new DummyEntity(Guid.NewGuid());
            var before = DateTime.UtcNow.AddMinutes(-1);
            var updatedAtProp = typeof(DummyEntity).BaseType!.GetProperty(
                "UpdatedAt",
                System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public
            );
            updatedAtProp!.SetValue(entity, before);
            entity
                .GetType()
                .GetMethod(
                    "SetUpdated",
                    System.Reflection.BindingFlags.NonPublic
                        | System.Reflection.BindingFlags.Instance
                )!
                .Invoke(entity, null);
            Assert.True(entity.UpdatedAt > before);
        }

        [Fact]
        public void Entity_CreatedAt_IsSet()
        {
            var entity = new DummyEntity(Guid.NewGuid());
            Assert.True((DateTime.UtcNow - entity.CreatedAt).TotalSeconds < 5);
        }

        [Fact]
        public void Entities_Equals_ObjectNull_ReturnsFalse()
        {
            var entity = new DummyEntity(Guid.NewGuid());
            Assert.False(entity.Equals((object?)null));
        }

        [Fact]
        public void Entities_Equals_OtherType_ReturnsFalse()
        {
            var entity = new DummyEntity(Guid.NewGuid());
            Assert.False(entity.Equals("not an entity"));
        }

        [Fact]
        public void Entities_Equals_EntityNull_ReturnsFalse()
        {
            var entity = new DummyEntity(Guid.NewGuid());
            Assert.False(entity.Equals((DummyEntity?)null));
        }

        [Fact]
        public void Entity_GetHashCode_IdNull_ReturnsZero()
        {
            var entity = new DummyEntity(Guid.Empty);
            entity.Id = Guid.Empty;
            Assert.Equal(0, entity.GetHashCode());
        }

        [Fact]
        public void Entity_Id_Default_DoesNotThrow()
        {
            var entity = new DummyEntity(Guid.Empty);
            Assert.NotNull(entity);
        }

        [Fact]
        public void Entity_UpdatedAt_Setter_WorksViaReflection()
        {
            var entity = new DummyEntity(Guid.NewGuid());
            var newDate = DateTime.UtcNow.AddDays(-1);
            var prop = typeof(DummyEntity).BaseType!.GetProperty(
                "UpdatedAt",
                System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public
            );
            prop!.SetValue(entity, newDate, null);
            Assert.Equal(newDate, entity.UpdatedAt);
        }

        [Fact]
        public void Entities_WithSameId_AreEqual()
        {
            var id = Guid.NewGuid();
            var e1 = new DummyEntity(id);
            var e2 = new DummyEntity(id);
            Assert.Equal(e1, e2);
            Assert.True(e1.Equals(e2));
            Assert.Equal(e1.GetHashCode(), e2.GetHashCode());
        }

        [Fact]
        public void Entities_WithDifferentId_AreNotEqual()
        {
            var e1 = new DummyEntity(Guid.NewGuid());
            var e2 = new DummyEntity(Guid.NewGuid());
            Assert.NotEqual(e1, e2);
            Assert.False(e1.Equals(e2));
        }

        [Fact]
        public void Entity_Equals_NullOrOtherType()
        {
            var e1 = new DummyEntity(Guid.NewGuid());
            Assert.False(e1.Equals(null));
            Assert.False(e1.Equals("string"));
        }

        [Fact]
        public void Entity_Equals_Self()
        {
            var e1 = new DummyEntity(Guid.NewGuid());
            Assert.True(e1.Equals(e1));
        }

        [Fact]
        public void Entity_Equals_WithDifferentTypes()
        {
            var entity = new DummyEntity(Guid.NewGuid());
            var differentType = new object();
            Assert.False(entity.Equals(differentType));
        }

        [Fact]
        public void Entity_Equals_WithNull()
        {
            var entity = new DummyEntity(Guid.NewGuid());
            Assert.False(entity.Equals((object?)null));
            Assert.False(entity.Equals((DummyEntity?)null));
        }
    }
}

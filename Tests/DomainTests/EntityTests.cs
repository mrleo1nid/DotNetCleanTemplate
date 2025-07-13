using DotNetCleanTemplate.Domain.Common;

namespace DomainTests
{
    public class DummyEntity : Entity<Guid>
    {
        public DummyEntity(Guid id)
        {
            Id = id;
        }
    }

    public class EntityTests
    {
        [Fact]
        public void Entities_WithSameId_AreEqual()
        {
            var id = Guid.NewGuid();
            var a = new DummyEntity(id);
            var b = new DummyEntity(id);
            Assert.Equal(a, b);
        }

        [Fact]
        public void Entities_WithDifferentId_AreNotEqual()
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
    }
}

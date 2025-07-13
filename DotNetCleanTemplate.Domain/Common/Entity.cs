namespace DotNetCleanTemplate.Domain.Common
{
    public abstract class Entity<TId> : IEquatable<Entity<TId>>
    {
        public TId Id { get; set; } = default!;
        public DateTime CreatedAt { get; init; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; private set; } = DateTime.UtcNow;

        public override int GetHashCode()
        {
            return Id is null ? 0 : EqualityComparer<TId>.Default.GetHashCode(Id);
        }

        public override bool Equals(object? obj)
        {
            if (obj is Entity<TId> entity)
            {
                return Equals(entity);
            }
            return false;
        }

        public virtual bool Equals(Entity<TId>? other)
        {
            if (other == null)
            {
                return false;
            }
            return EqualityComparer<TId>.Default.Equals(Id, other!.Id);
        }

        protected void SetUpdated()
        {
            UpdatedAt = DateTime.UtcNow;
        }
    }
}

using DotNetCleanTemplate.Domain.Common;

namespace DotNetCleanTemplate.Domain.ValueObjects.Role
{
    public class RoleName : ValueObject
    {
        public string Value { get; }

        public RoleName(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("Role name cannot be empty.");
            if (value.Length < DomainConstants.MinRoleNameLength)
                throw new ArgumentException("Role name is too short.");
            if (value.Length > DomainConstants.MaxRoleNameLength)
                throw new ArgumentException("Role name is too long.");
            Value = value.Trim();
        }

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return Value.ToLowerInvariant();
        }

        public override string ToString() => Value;
    }
}

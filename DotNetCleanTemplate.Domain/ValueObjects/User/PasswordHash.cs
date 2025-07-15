using DotNetCleanTemplate.Domain.Common;

namespace DotNetCleanTemplate.Domain.ValueObjects.User
{
    public class PasswordHash : ValueObject
    {
        public string Value { get; }

        public PasswordHash(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("Password hash cannot be empty.");
            if (value.Length > DomainConstants.MaxPasswordHashLength)
                throw new ArgumentException("Password hash is too long.");
            if (value.Length < DomainConstants.MinPasswordHashLength)
                throw new ArgumentException("Password hash is too short.");
            Value = value;
        }

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return Value;
        }

        public override string ToString() => Value;
    }
}

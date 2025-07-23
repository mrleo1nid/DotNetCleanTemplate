using DotNetCleanTemplate.Domain.Common;

namespace DotNetCleanTemplate.Domain.ValueObjects.User
{
    public class UserName : ValueObject
    {
        public string Value { get; }

        public UserName(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("Name cannot be empty.");
            if (value.Length < DomainConstants.MinUserNameLength)
                throw new ArgumentException("Name is too short.");
            if (value.Length > DomainConstants.MaxUserNameLength)
                throw new ArgumentException("Name is too long.");
            Value = value.Trim();
        }

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return Value.ToLowerInvariant();
        }

        public override string ToString() => Value;
    }
}

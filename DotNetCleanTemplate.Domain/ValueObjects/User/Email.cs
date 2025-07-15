using System.Text.RegularExpressions;
using DotNetCleanTemplate.Domain.Common;

namespace DotNetCleanTemplate.Domain.ValueObjects.User
{
    public partial class Email : ValueObject
    {
        public string Value { get; }

        public Email(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("Email cannot be empty.");
            if (!EmailRegex().IsMatch(value))
                throw new ArgumentException("Invalid email format.");
            if (value.Length < DomainConstants.MinEmailLength)
                throw new ArgumentException("Email is too short.");
            if (value.Length > DomainConstants.MaxEmailLength)
                throw new ArgumentException("Email is too long.");
            if (value.Contains(".."))
                throw new ArgumentException(
                    "Invalid email format: consecutive dots are not allowed."
                );
            Value = value;
        }

        [GeneratedRegex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$")]
        private static partial Regex EmailRegex();

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return Value.ToLowerInvariant();
        }

        public override string ToString() => Value;
    }
}

namespace DotNetCleanTemplate.Shared.Common
{
    public record Error(string Code, string Message, ErrorType Type)
    {
        public static readonly Error None = new(string.Empty, string.Empty, ErrorType.Unexpected);
        public static readonly Error NullValue = new(
            "General.Null",
            "Значение не может быть null",
            ErrorType.Validation
        );

        public bool IsEmpty =>
            Code == string.Empty && Message == string.Empty && Type == ErrorType.Unexpected;
    }
}

namespace DotNetCleanTemplate.Shared.Common
{
    public record Error
    {
        public Error(string Code, string Message, ErrorType Type)
        {
            this.Code = Code ?? throw new ArgumentNullException(nameof(Code));
            this.Message = Message ?? throw new ArgumentNullException(nameof(Message));
            this.Type = Type;
        }

        public string Code { get; }
        public string Message { get; }
        public ErrorType Type { get; }

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

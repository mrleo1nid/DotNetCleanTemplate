namespace DotNetCleanTemplate.Shared.Common
{
    public record Error(string Code, string Message)
    {
        public static readonly Error None = new(string.Empty, string.Empty);
        public static readonly Error NullValue = new("General.Null", "Значение не может быть null");

        public bool IsEmpty => Code == string.Empty && Message == string.Empty;
    }
}

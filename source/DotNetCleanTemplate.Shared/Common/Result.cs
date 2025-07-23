using MediatR;

namespace DotNetCleanTemplate.Shared.Common
{
    public class Result<T>
    {
        public bool IsSuccess { get; set; }
        public T Value { get; set; } = default!;
        public IReadOnlyList<Error> Errors { get; set; } = new List<Error>().AsReadOnly();

        // Публичный конструктор по умолчанию для System.Text.Json
        public Result() { }

        private Result(bool isSuccess, T value, IEnumerable<Error> errors)
        {
            if (isSuccess && value is null)
                throw new ArgumentNullException(
                    nameof(value),
                    "Value cannot be null for a successful result."
                );

            IsSuccess = isSuccess;
            Value = value!;
            Errors = errors?.ToList().AsReadOnly() ?? new List<Error>().AsReadOnly();
        }

        // --- Success ---
        public static Result<T> Success(T value) => new Result<T>(true, value, new List<Error>());

        public static Result<Unit> Success() =>
            new Result<Unit>(true, Unit.Value, new List<Error>());

        // --- Failure ---
        public static Result<T> Failure(IEnumerable<Error> errors) =>
            new Result<T>(false, default!, errors);

        public static Result<T> Failure(Error error) =>
            Failure(new[] { error ?? throw new ArgumentNullException(nameof(error)) });

        public static Result<T> Failure(string code, string message) =>
            Failure(new Error(code, message, ErrorType.Unexpected));

        public static Result<T> Failure(string code, string message, ErrorType type) =>
            Failure(new Error(code, message, type));
    }
}

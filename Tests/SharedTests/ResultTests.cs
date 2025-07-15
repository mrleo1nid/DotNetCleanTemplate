using DotNetCleanTemplate.Shared.Common;
using MediatR;

namespace SharedTests
{
    public class ResultTests
    {
        [Fact]
        public void Success_WithValue_CreatesSuccessfulResult()
        {
            var result = Result<int>.Success(42);
            Assert.True(result.IsSuccess);
            Assert.Equal(42, result.Value);
            Assert.Empty(result.Errors);
        }

        [Fact]
        public void Success_WithoutValue_CreatesSuccessfulUnitResult()
        {
            var result = Result<Unit>.Success();
            Assert.True(result.IsSuccess);
            Assert.Equal(Unit.Value, result.Value);
            Assert.Empty(result.Errors);
        }

        [Fact]
        public void Failure_WithError_CreatesFailedResult()
        {
            var error = new Error("code", "msg");
            var result = Result<int>.Failure(error);
            Assert.False(result.IsSuccess);
            Assert.Contains(error, result.Errors);
        }

        [Fact]
        public void Failure_WithMultipleErrors_CreatesFailedResult()
        {
            var errors = new[] { new Error("c1", "m1"), new Error("c2", "m2") };
            var result = Result<int>.Failure(errors);
            Assert.False(result.IsSuccess);
            Assert.Equal(errors, result.Errors);
        }

        [Fact]
        public void Failure_WithCodeAndMessage_CreatesFailedResult()
        {
            var result = Result<int>.Failure("c", "m");
            Assert.False(result.IsSuccess);
            Assert.Single(result.Errors);
            Assert.Equal("c", result.Errors[0].Code);
            Assert.Equal("m", result.Errors[0].Message);
        }

        [Fact]
        public void Success_WithNullValue_ThrowsException()
        {
            Assert.Throws<ArgumentNullException>(() => Result<string>.Success(null!));
        }
    }
}

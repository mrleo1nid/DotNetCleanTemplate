using DotNetCleanTemplate.Shared.Common;

namespace SharedTests
{
    public class ResultTests
    {
        [Fact]
        public void Success_ShouldReturnIsSuccessTrue()
        {
            var result = Result<string>.Success("ok");
            Assert.True(result.IsSuccess);
            Assert.Equal("ok", result.Value);
        }

        [Fact]
        public void Failure_ShouldReturnIsSuccessFalse()
        {
            var result = Result<string>.Failure(Error.NullValue);
            Assert.False(result.IsSuccess);
        }

        [Fact]
        public void Failure_ShouldContainError()
        {
            var error = Error.NullValue;
            var result = Result<string>.Failure(error);
            Assert.Contains(error, result.Errors);
        }

        [Fact]
        public void Success_ShouldThrow_WhenValueIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => Result<string>.Success(null!));
        }

        [Fact]
        public void Failure_ShouldContainMultipleErrors()
        {
            var errors = new[] { new Error("E1", "msg1"), new Error("E2", "msg2") };
            var result = Result<string>.Failure(errors);
            Assert.Equal(2, result.Errors.Count);
        }

        [Fact]
        public void Failure_ByCodeAndMessage_ShouldContainError()
        {
            var result = Result<string>.Failure("E1", "msg1");
            Assert.Single(result.Errors);
            Assert.Equal("E1", result.Errors[0].Code);
            Assert.Equal("msg1", result.Errors[0].Message);
        }
    }
}

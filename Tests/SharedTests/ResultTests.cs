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
    }
}

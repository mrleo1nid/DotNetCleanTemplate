using DotNetCleanTemplate.Shared.Common;

namespace SharedTests
{
    public class ErrorTypeTests
    {
        [Theory]
        [InlineData(ErrorType.Validation)]
        [InlineData(ErrorType.NotFound)]
        [InlineData(ErrorType.Conflict)]
        [InlineData(ErrorType.Unauthorized)]
        [InlineData(ErrorType.Forbidden)]
        [InlineData(ErrorType.Unexpected)]
        public void ErrorType_Enum_HasExpectedValues(ErrorType errorType)
        {
            Assert.True(Enum.IsDefined(errorType));
        }
    }
}

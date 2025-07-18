using DotNetCleanTemplate.Shared.Common;

namespace DotNetCleanTemplate.UnitTests.Shared
{
    public class ErrorTests
    {
        [Fact]
        public void Error_CanBeCreated()
        {
            var error = new Error("Code1", "Message1", ErrorType.Unexpected);
            Assert.Equal("Code1", error.Code);
            Assert.Equal("Message1", error.Message);
        }

        [Fact]
        public void Error_None_IsEmpty()
        {
            Assert.True(Error.None.IsEmpty);
            Assert.Equal(string.Empty, Error.None.Code);
            Assert.Equal(string.Empty, Error.None.Message);
        }

        [Fact]
        public void Error_NullValue_IsNotEmpty()
        {
            Assert.False(Error.NullValue.IsEmpty);
            Assert.Equal("General.Null", Error.NullValue.Code);
            Assert.Equal("Значение не может быть null", Error.NullValue.Message);
        }

        [Fact]
        public void ErrorsWithSameTypeAndMessage_ShouldBeEqual()
        {
            var a = new Error("Validation", "msg", ErrorType.Validation);
            var b = new Error("Validation", "msg", ErrorType.Validation);
            Assert.Equal(a, b);
        }

        [Fact]
        public void ErrorsWithDifferentTypeOrMessage_ShouldNotBeEqual()
        {
            var a = new Error("Validation", "msg", ErrorType.Validation);
            var b = new Error("NotFound", "msg", ErrorType.NotFound);
            var c = new Error("Validation", "other", ErrorType.Validation);
            Assert.NotEqual(a, b);
            Assert.NotEqual(a, c);
        }
    }
}

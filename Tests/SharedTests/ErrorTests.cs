using DotNetCleanTemplate.Shared.Common;
using Xunit;

namespace SharedTests
{
    public class ErrorTests
    {
        [Fact]
        public void Error_CanBeCreated()
        {
            var error = new Error("Code1", "Message1");
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
    }
}

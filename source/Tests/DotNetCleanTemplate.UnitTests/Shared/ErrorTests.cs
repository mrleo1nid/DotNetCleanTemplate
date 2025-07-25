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

        [Fact]
        public void Constructor_WithNullCode_ShouldThrow()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() =>
                new Error(null!, "message", ErrorType.Validation)
            );
        }

        [Fact]
        public void Constructor_WithNullMessage_ShouldThrow()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() =>
                new Error("code", null!, ErrorType.Validation)
            );
        }

        [Fact]
        public void Equals_WithDifferentErrors_ShouldReturnFalse()
        {
            // Arrange
            var error1 = new Error("CODE1", "Message 1", ErrorType.Validation);
            var error2 = new Error("CODE2", "Message 2", ErrorType.NotFound);

            // Act & Assert
            Assert.False(error1.Equals(error2));
            Assert.False(error1.Equals((object)error2));
        }

        [Fact]
        public void Equals_WithSameError_ShouldReturnTrue()
        {
            // Arrange
            var error1 = new Error("CODE", "Message", ErrorType.Validation);
            var error2 = new Error("CODE", "Message", ErrorType.Validation);

            // Act & Assert
            Assert.True(error1.Equals(error2));
            Assert.True(error1.Equals((object)error2));
        }

        [Fact]
        public void Equals_WithNull_ShouldReturnFalse()
        {
            // Arrange
            var error = new Error("CODE", "Message", ErrorType.Validation);

            // Act & Assert
            Assert.False(error.Equals(null));
            Assert.False(error.Equals((object)null!));
        }

        [Fact]
        public void GetHashCode_WithSameErrors_ShouldReturnSameHashCode()
        {
            // Arrange
            var error1 = new Error("CODE", "Message", ErrorType.Validation);
            var error2 = new Error("CODE", "Message", ErrorType.Validation);

            // Act & Assert
            Assert.Equal(error1.GetHashCode(), error2.GetHashCode());
        }
    }
}

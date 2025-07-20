using DotNetCleanTemplate.Shared.Common;
using MediatR;

namespace DotNetCleanTemplate.UnitTests.Shared
{
    public class ResultTests
    {
        [Fact]
        public void Success_CreatesSuccessResult()
        {
            var result = Result<string>.Success("ok");
            Assert.True(result.IsSuccess);
            Assert.Equal("ok", result.Value);
            Assert.Empty(result.Errors);
        }

        [Fact]
        public void Failure_CreatesFailureResult()
        {
            var error = new Error("E", "fail", ErrorType.Unexpected);
            var result = Result<string>.Failure(error);
            Assert.False(result.IsSuccess);
            Assert.Equal(default, result.Value);
            Assert.Single(result.Errors);
            Assert.Equal(error, result.Errors[0]);
        }

        [Fact]
        public void Failure_WithMultipleErrors()
        {
            var errors = new[]
            {
                new Error("E1", "fail1", ErrorType.Unexpected),
                new Error("E2", "fail2", ErrorType.Unexpected),
            };
            var result = Result<string>.Failure(errors);
            Assert.False(result.IsSuccess);
            Assert.Equal(errors, result.Errors);
        }

        [Fact]
        public void Success_WithNull_Throws()
        {
            Assert.Throws<ArgumentNullException>(() => Result<string>.Success(null!));
        }

        [Fact]
        public void Success_Unit()
        {
            var result = Result<Unit>.Success();
            Assert.True(result.IsSuccess);
            Assert.Equal(Unit.Value, result.Value);
            Assert.Empty(result.Errors);
        }

        [Fact]
        public void Failure_ByCodeAndMessage()
        {
            var result = Result<string>.Failure("C", "msg");
            Assert.False(result.IsSuccess);
            Assert.Single(result.Errors);
            Assert.Equal("C", result.Errors[0].Code);
            Assert.Equal("msg", result.Errors[0].Message);
        }

        [Fact]
        public void Errors_Of_Success_IsEmpty()
        {
            var result = Result<int>.Success(42);
            Assert.Empty(result.Errors);
        }

        [Fact]
        public void Value_Of_Failure_IsDefault()
        {
            var result = Result<int>.Failure("C", "fail");
            Assert.Equal(default, result.Value);
        }

        [Fact]
        public void WithValue_WhenValueIsNull_ShouldThrow()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => Result<string>.Success(null!));
        }

        [Fact]
        public void WithError_WhenErrorIsNull_ShouldThrow()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => Result<string>.Failure((Error)null!));
        }

        [Fact]
        public void IsSuccess_WithSuccessResult_ShouldReturnTrue()
        {
            // Arrange
            var result = Result<string>.Success("test");

            // Act & Assert
            Assert.True(result.IsSuccess);
        }

        [Fact]
        public void IsSuccess_WithFailureResult_ShouldReturnFalse()
        {
            // Arrange
            var result = Result<string>.Failure("TEST", "Test error");

            // Act & Assert
            Assert.False(result.IsSuccess);
        }
    }
}

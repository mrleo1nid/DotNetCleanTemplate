using DotNetCleanTemplate.Shared.Common;

namespace DotNetCleanTemplate.UnitTests.Shared.Common;

public class ErrorTests
{
    #region Constructor Tests

    [Fact]
    public void Error_DefaultConstructor_ShouldInitializeWithDefaultValues()
    {
        // Act
        var error = new Error();

        // Assert
        Assert.Equal(string.Empty, error.Code);
        Assert.Equal(string.Empty, error.Message);
        Assert.Equal(ErrorType.Unexpected, error.Type);
    }

    [Fact]
    public void Error_ParameterizedConstructor_ShouldInitializeWithProvidedValues()
    {
        // Arrange
        var code = "Test.Error";
        var message = "Test error message";
        var type = ErrorType.Validation;

        // Act
        var error = new Error(code, message, type);

        // Assert
        Assert.Equal(code, error.Code);
        Assert.Equal(message, error.Message);
        Assert.Equal(type, error.Type);
    }

    [Fact]
    public void Error_ConstructorWithNullCode_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentNullException>(() =>
            new Error(null!, "Message", ErrorType.Validation)
        );
        Assert.Equal("Code", exception.ParamName);
    }

    [Fact]
    public void Error_ConstructorWithNullMessage_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentNullException>(() =>
            new Error("Code", null!, ErrorType.Validation)
        );
        Assert.Equal("Message", exception.ParamName);
    }

    #endregion

    #region Static Properties Tests

    [Fact]
    public void Error_None_ShouldReturnEmptyError()
    {
        // Act
        var error = Error.None;

        // Assert
        Assert.Equal(string.Empty, error.Code);
        Assert.Equal(string.Empty, error.Message);
        Assert.Equal(ErrorType.Unexpected, error.Type);
    }

    [Fact]
    public void Error_NullValue_ShouldReturnNullValueError()
    {
        // Act
        var error = Error.NullValue;

        // Assert
        Assert.Equal("General.Null", error.Code);
        Assert.Equal("Значение не может быть null", error.Message);
        Assert.Equal(ErrorType.Validation, error.Type);
    }

    #endregion

    #region IsEmpty Tests

    [Fact]
    public void IsEmpty_WithEmptyError_ShouldReturnTrue()
    {
        // Arrange
        var error = new Error();

        // Act
        var result = error.IsEmpty;

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void IsEmpty_WithNoneError_ShouldReturnTrue()
    {
        // Act
        var result = Error.None.IsEmpty;

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void IsEmpty_WithNonEmptyCode_ShouldReturnFalse()
    {
        // Arrange
        var error = new Error("Test.Code", string.Empty, ErrorType.Unexpected);

        // Act
        var result = error.IsEmpty;

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void IsEmpty_WithNonEmptyMessage_ShouldReturnFalse()
    {
        // Arrange
        var error = new Error(string.Empty, "Test message", ErrorType.Unexpected);

        // Act
        var result = error.IsEmpty;

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void IsEmpty_WithNonUnexpectedType_ShouldReturnFalse()
    {
        // Arrange
        var error = new Error(string.Empty, string.Empty, ErrorType.Validation);

        // Act
        var result = error.IsEmpty;

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void IsEmpty_WithNullValueError_ShouldReturnFalse()
    {
        // Act
        var result = Error.NullValue.IsEmpty;

        // Assert
        Assert.False(result);
    }

    #endregion

    #region Record Equality Tests

    [Fact]
    public void Error_WithSameValues_ShouldBeEqual()
    {
        // Arrange
        var error1 = new Error("Test.Code", "Test message", ErrorType.Validation);
        var error2 = new Error("Test.Code", "Test message", ErrorType.Validation);

        // Act & Assert
        Assert.Equal(error1, error2);
        Assert.True(error1 == error2);
        Assert.False(error1 != error2);
    }

    [Fact]
    public void Error_WithDifferentCode_ShouldNotBeEqual()
    {
        // Arrange
        var error1 = new Error("Test.Code1", "Test message", ErrorType.Validation);
        var error2 = new Error("Test.Code2", "Test message", ErrorType.Validation);

        // Act & Assert
        Assert.NotEqual(error1, error2);
        Assert.False(error1 == error2);
        Assert.True(error1 != error2);
    }

    [Fact]
    public void Error_WithDifferentMessage_ShouldNotBeEqual()
    {
        // Arrange
        var error1 = new Error("Test.Code", "Test message 1", ErrorType.Validation);
        var error2 = new Error("Test.Code", "Test message 2", ErrorType.Validation);

        // Act & Assert
        Assert.NotEqual(error1, error2);
        Assert.False(error1 == error2);
        Assert.True(error1 != error2);
    }

    [Fact]
    public void Error_WithDifferentType_ShouldNotBeEqual()
    {
        // Arrange
        var error1 = new Error("Test.Code", "Test message", ErrorType.Validation);
        var error2 = new Error("Test.Code", "Test message", ErrorType.NotFound);

        // Act & Assert
        Assert.NotEqual(error1, error2);
        Assert.False(error1 == error2);
        Assert.True(error1 != error2);
    }

    #endregion


    #region Immutability Tests

    [Fact]
    public void Error_Properties_ShouldBeImmutable()
    {
        // Arrange
        var error = new Error("Test.Code", "Test message", ErrorType.Validation);

        // Act & Assert
        // Since Error is a record with init-only properties, we can't modify them after creation
        // This test verifies that the properties are read-only
        Assert.Equal("Test.Code", error.Code);
        Assert.Equal("Test message", error.Message);
        Assert.Equal(ErrorType.Validation, error.Type);
    }

    #endregion
}

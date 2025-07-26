using DotNetCleanTemplate.Application.Validation;
using DotNetCleanTemplate.Shared.DTOs;
using FluentValidation.TestHelper;

namespace DotNetCleanTemplate.UnitTests.Application.Validation;

public class RegisterUserDtoValidatorTests
{
    private readonly RegisterUserDtoValidator _validator;

    public RegisterUserDtoValidatorTests()
    {
        _validator = new RegisterUserDtoValidator();
    }

    [Fact]
    public void Constructor_Should_Initialize_Validators()
    {
        // Assert
        Assert.NotNull(_validator);
    }

    [Fact]
    public void Validate_WithValidData_ShouldPass()
    {
        // Arrange
        var dto = new RegisterUserDto
        {
            UserName = "testuser",
            Email = "test@example.com",
            Password = "ValidPassword123!",
        };

        // Act
        var result = _validator.TestValidate(dto);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_WithEmptyEmail_ShouldFail()
    {
        // Arrange
        var dto = new RegisterUserDto
        {
            UserName = "testuser",
            Email = string.Empty,
            Password = "ValidPassword123!",
        };

        // Act
        var result = _validator.TestValidate(dto);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Email);
    }

    [Fact]
    public void Validate_WithInvalidEmail_ShouldFail()
    {
        // Arrange
        var dto = new RegisterUserDto
        {
            UserName = "testuser",
            Email = "invalid-email",
            Password = "ValidPassword123!",
        };

        // Act
        var result = _validator.TestValidate(dto);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Email);
    }

    [Fact]
    public void Validate_WithEmptyName_ShouldFail()
    {
        // Arrange
        var dto = new RegisterUserDto
        {
            UserName = string.Empty,
            Email = "test@example.com",
            Password = "ValidPassword123!",
        };

        // Act
        var result = _validator.TestValidate(dto);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.UserName);
    }

    [Fact]
    public void Validate_WithShortName_ShouldFail()
    {
        // Arrange
        var dto = new RegisterUserDto
        {
            UserName = "ab",
            Email = "test@example.com",
            Password = "ValidPassword123!",
        };

        // Act
        var result = _validator.TestValidate(dto);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.UserName);
    }

    [Fact]
    public void Validate_WithLongName_ShouldFail()
    {
        // Arrange
        var dto = new RegisterUserDto
        {
            UserName = new string('a', 256),
            Email = "test@example.com",
            Password = "ValidPassword123!",
        };

        // Act
        var result = _validator.TestValidate(dto);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.UserName);
    }

    [Fact]
    public void Validate_WithEmptyPassword_ShouldFail()
    {
        // Arrange
        var dto = new RegisterUserDto
        {
            UserName = "testuser",
            Email = "test@example.com",
            Password = string.Empty,
        };

        // Act
        var result = _validator.TestValidate(dto);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Password);
    }

    [Fact]
    public void Validate_WithWeakPassword_ShouldFail()
    {
        // Arrange
        var dto = new RegisterUserDto
        {
            UserName = "testuser",
            Email = "test@example.com",
            Password = "123",
        };

        // Act
        var result = _validator.TestValidate(dto);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Password);
    }
}

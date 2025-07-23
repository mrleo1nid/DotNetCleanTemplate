using DotNetCleanTemplate.Application.Validation;
using DotNetCleanTemplate.Shared.DTOs;
using FluentValidation.TestHelper;
using Xunit;

namespace DotNetCleanTemplate.UnitTests.Application.Validation;

public class LoginRequestDtoValidatorTests
{
    private readonly LoginRequestDtoValidator _validator;

    public LoginRequestDtoValidatorTests()
    {
        _validator = new LoginRequestDtoValidator();
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
        var dto = new LoginRequestDto
        {
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
        var dto = new LoginRequestDto { Email = string.Empty, Password = "ValidPassword123!" };

        // Act
        var result = _validator.TestValidate(dto);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Email);
    }

    [Fact]
    public void Validate_WithInvalidEmail_ShouldFail()
    {
        // Arrange
        var dto = new LoginRequestDto { Email = "invalid-email", Password = "ValidPassword123!" };

        // Act
        var result = _validator.TestValidate(dto);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Email);
    }

    [Fact]
    public void Validate_WithEmptyPassword_ShouldFail()
    {
        // Arrange
        var dto = new LoginRequestDto { Email = "test@example.com", Password = string.Empty };

        // Act
        var result = _validator.TestValidate(dto);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Password);
    }

    [Fact]
    public void Validate_WithShortPassword_ShouldFail()
    {
        // Arrange
        var dto = new LoginRequestDto { Email = "test@example.com", Password = "123" };

        // Act
        var result = _validator.TestValidate(dto);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Password);
    }

    [Fact]
    public void Validate_WithLongEmail_ShouldFail()
    {
        // Arrange
        var dto = new LoginRequestDto
        {
            Email = new string('a', 256) + "@example.com",
            Password = "ValidPassword123!",
        };

        // Act
        var result = _validator.TestValidate(dto);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Email);
    }
}

using DotNetCleanTemplate.Application.Validation;
using DotNetCleanTemplate.Shared.DTOs;
using FluentValidation.TestHelper;
using Xunit;

namespace DotNetCleanTemplate.UnitTests.Application.Validation;

public class CreateUserDtoValidatorTests
{
    private readonly CreateUserDtoValidator _validator;

    public CreateUserDtoValidatorTests()
    {
        _validator = new CreateUserDtoValidator();
    }

    [Fact]
    public void Should_Pass_When_Valid_Data()
    {
        // Arrange
        var dto = new CreateUserDto
        {
            UserName = "testuser",
            Email = "test@example.com",
            Password = "Password123",
            ConfirmPassword = "Password123",
        };

        // Act
        var result = _validator.TestValidate(dto);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Should_Fail_When_UserName_Empty()
    {
        // Arrange
        var dto = new CreateUserDto
        {
            UserName = "",
            Email = "test@example.com",
            Password = "Password123",
            ConfirmPassword = "Password123",
        };

        // Act
        var result = _validator.TestValidate(dto);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.UserName);
    }

    [Fact]
    public void Should_Fail_When_UserName_Too_Short()
    {
        // Arrange
        var dto = new CreateUserDto
        {
            UserName = "ab",
            Email = "test@example.com",
            Password = "Password123",
            ConfirmPassword = "Password123",
        };

        // Act
        var result = _validator.TestValidate(dto);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.UserName);
    }

    [Fact]
    public void Should_Fail_When_UserName_Contains_Invalid_Characters()
    {
        // Arrange
        var dto = new CreateUserDto
        {
            UserName = "test@user",
            Email = "test@example.com",
            Password = "Password123",
            ConfirmPassword = "Password123",
        };

        // Act
        var result = _validator.TestValidate(dto);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.UserName);
    }

    [Fact]
    public void Should_Fail_When_Email_Invalid()
    {
        // Arrange
        var dto = new CreateUserDto
        {
            UserName = "testuser",
            Email = "invalid-email",
            Password = "Password123",
            ConfirmPassword = "Password123",
        };

        // Act
        var result = _validator.TestValidate(dto);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Email);
    }

    [Fact]
    public void Should_Fail_When_Password_Too_Short()
    {
        // Arrange
        var dto = new CreateUserDto
        {
            UserName = "testuser",
            Email = "test@example.com",
            Password = "123",
            ConfirmPassword = "123",
        };

        // Act
        var result = _validator.TestValidate(dto);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Password);
    }

    [Fact]
    public void Should_Fail_When_Password_Missing_Uppercase()
    {
        // Arrange
        var dto = new CreateUserDto
        {
            UserName = "testuser",
            Email = "test@example.com",
            Password = "password123",
            ConfirmPassword = "password123",
        };

        // Act
        var result = _validator.TestValidate(dto);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Password);
    }

    [Fact]
    public void Should_Fail_When_Password_Missing_Lowercase()
    {
        // Arrange
        var dto = new CreateUserDto
        {
            UserName = "testuser",
            Email = "test@example.com",
            Password = "PASSWORD123",
            ConfirmPassword = "PASSWORD123",
        };

        // Act
        var result = _validator.TestValidate(dto);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Password);
    }

    [Fact]
    public void Should_Fail_When_Password_Missing_Digit()
    {
        // Arrange
        var dto = new CreateUserDto
        {
            UserName = "testuser",
            Email = "test@example.com",
            Password = "Password",
            ConfirmPassword = "Password",
        };

        // Act
        var result = _validator.TestValidate(dto);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Password);
    }

    [Fact]
    public void Should_Fail_When_Passwords_Do_Not_Match()
    {
        // Arrange
        var dto = new CreateUserDto
        {
            UserName = "testuser",
            Email = "test@example.com",
            Password = "Password123",
            ConfirmPassword = "Password456",
        };

        // Act
        var result = _validator.TestValidate(dto);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.ConfirmPassword);
    }

    [Fact]
    public void Should_Fail_When_ConfirmPassword_Empty()
    {
        // Arrange
        var dto = new CreateUserDto
        {
            UserName = "testuser",
            Email = "test@example.com",
            Password = "Password123",
            ConfirmPassword = "",
        };

        // Act
        var result = _validator.TestValidate(dto);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.ConfirmPassword);
    }
}

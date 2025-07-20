using DotNetCleanTemplate.Application.Validation;
using DotNetCleanTemplate.Shared.DTOs;
using FluentValidation.TestHelper;
using Xunit;

namespace DotNetCleanTemplate.UnitTests.Application.Validation;

public class LoginResponseDtoValidatorTests
{
    private readonly LoginResponseDtoValidator _validator;

    public LoginResponseDtoValidatorTests()
    {
        _validator = new LoginResponseDtoValidator();
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
        var dto = new LoginResponseDto
        {
            AccessToken = "valid.jwt.token",
            RefreshToken = "valid.refresh.token",
            RefreshTokenExpires = DateTime.UtcNow.AddDays(7),
        };

        // Act
        var result = _validator.TestValidate(dto);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_WithEmptyToken_ShouldFail()
    {
        // Arrange
        var dto = new LoginResponseDto
        {
            AccessToken = string.Empty,
            RefreshToken = "valid.refresh.token",
            RefreshTokenExpires = DateTime.UtcNow.AddDays(7),
        };

        // Act
        var result = _validator.TestValidate(dto);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.AccessToken);
    }

    [Fact]
    public void Validate_WithEmptyRefreshToken_ShouldFail()
    {
        // Arrange
        var dto = new LoginResponseDto
        {
            AccessToken = "valid.jwt.token",
            RefreshToken = string.Empty,
            RefreshTokenExpires = DateTime.UtcNow.AddDays(7),
        };

        // Act
        var result = _validator.TestValidate(dto);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.RefreshToken);
    }
}

using DotNetCleanTemplate.Application.Validation;
using DotNetCleanTemplate.Shared.DTOs;
using FluentValidation.TestHelper;
using Xunit;

namespace DotNetCleanTemplate.UnitTests.Application.Validation;

public class RefreshTokenResponseDtoValidatorTests
{
    private readonly RefreshTokenResponseDtoValidator _validator;

    public RefreshTokenResponseDtoValidatorTests()
    {
        _validator = new RefreshTokenResponseDtoValidator();
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
        var dto = new RefreshTokenResponseDto
        {
            AccessToken = "valid.jwt.token",
            RefreshToken = "valid.refresh.token",
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
        var dto = new RefreshTokenResponseDto { AccessToken = string.Empty };

        // Act
        var result = _validator.TestValidate(dto);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.AccessToken);
    }

    [Fact]
    public void Validate_WithEmptyRefreshToken_ShouldFail()
    {
        // Arrange
        var dto = new RefreshTokenResponseDto
        {
            AccessToken = "valid.jwt.token",
            RefreshToken = string.Empty,
        };

        // Act
        var result = _validator.TestValidate(dto);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.RefreshToken);
    }
}

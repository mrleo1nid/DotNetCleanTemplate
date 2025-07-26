using DotNetCleanTemplate.Application.Validation;
using DotNetCleanTemplate.Shared.DTOs;
using FluentValidation.TestHelper;

namespace DotNetCleanTemplate.UnitTests.Application.Validation;

public class AssignRoleToUserDtoValidatorTests
{
    private readonly AssignRoleToUserDtoValidator _validator;

    public AssignRoleToUserDtoValidatorTests()
    {
        _validator = new AssignRoleToUserDtoValidator();
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
        var dto = new AssignRoleToUserDto { UserId = Guid.NewGuid(), RoleId = Guid.NewGuid() };

        // Act
        var result = _validator.TestValidate(dto);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_WithEmptyUserId_ShouldFail()
    {
        // Arrange
        var dto = new AssignRoleToUserDto { UserId = Guid.Empty, RoleId = Guid.NewGuid() };

        // Act
        var result = _validator.TestValidate(dto);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.UserId);
    }

    [Fact]
    public void Validate_WithEmptyRoleId_ShouldFail()
    {
        // Arrange
        var dto = new AssignRoleToUserDto { UserId = Guid.NewGuid(), RoleId = Guid.Empty };

        // Act
        var result = _validator.TestValidate(dto);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.RoleId);
    }

    [Fact]
    public void Validate_WithInvalidGuid_ShouldFail()
    {
        // Arrange
        var dto = new AssignRoleToUserDto { UserId = Guid.Empty, RoleId = Guid.Empty };

        // Act
        var result = _validator.TestValidate(dto);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.UserId);
        result.ShouldHaveValidationErrorFor(x => x.RoleId);
    }
}

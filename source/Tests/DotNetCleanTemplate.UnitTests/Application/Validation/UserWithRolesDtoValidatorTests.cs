using DotNetCleanTemplate.Application.Validation;
using DotNetCleanTemplate.Shared.DTOs;
using FluentValidation.TestHelper;
using Xunit;

namespace DotNetCleanTemplate.UnitTests.Application.Validation;

public class UserWithRolesDtoValidatorTests
{
    private readonly UserWithRolesDtoValidator _validator;

    public UserWithRolesDtoValidatorTests()
    {
        _validator = new UserWithRolesDtoValidator();
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
        var dto = new UserWithRolesDto
        {
            Id = Guid.NewGuid(),
            UserName = "testuser",
            Email = "test@example.com",
            Roles = new List<RoleDto>
            {
                new() { Id = Guid.NewGuid(), Name = "Admin" },
            },
        };

        // Act
        var result = _validator.TestValidate(dto);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_WithEmptyId_ShouldPass()
    {
        // Arrange
        var dto = new UserWithRolesDto
        {
            Id = Guid.Empty,
            UserName = "testuser",
            Email = "test@example.com",
            Roles = new List<RoleDto>(),
        };

        // Act
        var result = _validator.TestValidate(dto);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Id);
    }

    [Fact]
    public void Validate_WithEmptyEmail_ShouldFail()
    {
        // Arrange
        var dto = new UserWithRolesDto
        {
            Id = Guid.NewGuid(),
            UserName = "testuser",
            Email = string.Empty,
            Roles = new List<RoleDto>(),
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
        var dto = new UserWithRolesDto
        {
            Id = Guid.NewGuid(),
            UserName = "testuser",
            Email = "invalid-email",
            Roles = new List<RoleDto>(),
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
        var dto = new UserWithRolesDto
        {
            Id = Guid.NewGuid(),
            UserName = string.Empty,
            Email = "test@example.com",
            Roles = new List<RoleDto>(),
        };

        // Act
        var result = _validator.TestValidate(dto);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.UserName);
    }

    [Fact]
    public void Validate_WithNullRoles_ShouldPass()
    {
        // Arrange
        var dto = new UserWithRolesDto
        {
            Id = Guid.NewGuid(),
            UserName = "testuser",
            Email = "test@example.com",
            Roles = null!,
        };

        // Act
        var result = _validator.TestValidate(dto);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Roles);
    }
}

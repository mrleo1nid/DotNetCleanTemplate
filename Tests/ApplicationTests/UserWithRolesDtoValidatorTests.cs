using System.Globalization;
using DotNetCleanTemplate.Application.Validation;
using DotNetCleanTemplate.Shared.DTOs;
using FluentAssertions;
using FluentValidation.TestHelper;

namespace ApplicationTests
{
    public class UserWithRolesDtoValidatorTests
    {
        static UserWithRolesDtoValidatorTests()
        {
            CultureInfo.DefaultThreadCurrentCulture = new CultureInfo("en-US");
            CultureInfo.DefaultThreadCurrentUICulture = new CultureInfo("en-US");
        }

        private readonly UserWithRolesDtoValidator _validator = new();

        [Fact]
        public void Should_Pass_For_Valid_Dto()
        {
            var dto = new UserWithRolesDto
            {
                Id = Guid.NewGuid(),
                UserName = "ValidUser",
                Email = "user@example.com",
                Roles = new List<RoleDto>
                {
                    new RoleDto { Id = Guid.NewGuid(), Name = "Admin" },
                },
            };
            var result = _validator.TestValidate(dto);
            result.ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public void Should_Fail_For_Invalid_Role()
        {
            // Arrange
            var dto = new UserWithRolesDto
            {
                UserName = "",
                Roles = new List<RoleDto> { new RoleDto { Name = "" } },
            };
            var validator = new UserWithRolesDtoValidator();

            // Act
            var result = validator.Validate(dto);

            // Debug: Вывести все ошибки для диагностики
            foreach (var error in result.Errors)
                Console.WriteLine($"Property: {error.PropertyName}, Message: {error.ErrorMessage}");

            // Assert
            result.Errors.Should().Contain(e => e.ErrorMessage.Contains("must not be empty"));
            result.Errors.Should().Contain(e => e.ErrorMessage.Contains("at least 3 characters"));
        }

        [Theory]
        [InlineData("")]
        [InlineData("ab")]
        public void Should_Fail_For_Invalid_UserName(string userName)
        {
            var dto = new UserWithRolesDto
            {
                Id = System.Guid.NewGuid(),
                UserName = userName,
                Email = "user@example.com",
                Roles = new System.Collections.Generic.List<RoleDto>
                {
                    new RoleDto { Id = System.Guid.NewGuid(), Name = "Admin" },
                },
            };
            var result = _validator.TestValidate(dto);
            result.ShouldHaveValidationErrorFor(x => x.UserName);
        }

        [Fact]
        public void Should_Fail_For_Null_UserName()
        {
            var dto = new UserWithRolesDto
            {
                Id = System.Guid.NewGuid(),
                UserName = null!,
                Email = "user@example.com",
                Roles = new System.Collections.Generic.List<RoleDto>
                {
                    new RoleDto { Id = System.Guid.NewGuid(), Name = "Admin" },
                },
            };
            var result = _validator.TestValidate(dto);
            result.ShouldHaveValidationErrorFor(x => x.UserName);
        }

        [Theory]
        [InlineData("")]
        [InlineData("bademail")]
        public void Should_Fail_For_Invalid_Email(string email)
        {
            var dto = new UserWithRolesDto
            {
                Id = System.Guid.NewGuid(),
                UserName = "ValidUser",
                Email = email,
                Roles = new System.Collections.Generic.List<RoleDto>
                {
                    new RoleDto { Id = System.Guid.NewGuid(), Name = "Admin" },
                },
            };
            var result = _validator.TestValidate(dto);
            result.ShouldHaveValidationErrorFor(x => x.Email);
        }

        [Fact]
        public void Should_Fail_For_Null_Email()
        {
            var dto = new UserWithRolesDto
            {
                Id = System.Guid.NewGuid(),
                UserName = "ValidUser",
                Email = null!,
                Roles = new System.Collections.Generic.List<RoleDto>
                {
                    new RoleDto { Id = System.Guid.NewGuid(), Name = "Admin" },
                },
            };
            var result = _validator.TestValidate(dto);
            result.ShouldHaveValidationErrorFor(x => x.Email);
        }
    }
}

using System;
using System.Collections.Generic;
using DotNetCleanTemplate.Application.Validation;
using DotNetCleanTemplate.Shared.DTOs;
using FluentAssertions;
using FluentValidation.TestHelper;
using Xunit;

namespace ApplicationTests
{
    public class UserWithRolesDtoValidatorTests
    {
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
            var dto = new UserWithRolesDto
            {
                Id = Guid.NewGuid(),
                UserName = "ValidUser",
                Email = "user@example.com",
                Roles = new List<RoleDto>
                {
                    new RoleDto { Id = Guid.NewGuid(), Name = "" },
                },
            };
            var result = _validator.TestValidate(dto);
            result.Errors.Should().Contain(e => e.PropertyName == "Roles[0].Name");
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

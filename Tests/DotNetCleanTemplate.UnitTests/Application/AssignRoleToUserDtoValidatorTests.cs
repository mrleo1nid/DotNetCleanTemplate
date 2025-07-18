using DotNetCleanTemplate.Application.Validation;
using DotNetCleanTemplate.Shared.DTOs;
using FluentValidation.TestHelper;

namespace DotNetCleanTemplate.UnitTests.Application
{
    public class AssignRoleToUserDtoValidatorTests
    {
        private readonly AssignRoleToUserDtoValidator _validator = new();

        [Fact]
        public void Should_Pass_Validation_For_Valid_Dto()
        {
            var dto = new AssignRoleToUserDto { UserId = Guid.NewGuid(), RoleId = Guid.NewGuid() };
            var result = _validator.TestValidate(dto);
            result.ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public void Should_Fail_Validation_For_Empty_Ids()
        {
            var dto = new AssignRoleToUserDto { UserId = Guid.Empty, RoleId = Guid.Empty };
            var result = _validator.TestValidate(dto);
            result.ShouldHaveValidationErrorFor(x => x.UserId);
            result.ShouldHaveValidationErrorFor(x => x.RoleId);
        }
    }
}

using DotNetCleanTemplate.Shared.DTOs;
using FluentValidation;

namespace DotNetCleanTemplate.Application.Validation
{
    public class AssignRoleToUserDtoValidator : AbstractValidator<AssignRoleToUserDto>
    {
        public AssignRoleToUserDtoValidator()
        {
            RuleFor(x => x.UserId).NotEmpty().WithMessage("UserId is required");
            RuleFor(x => x.RoleId).NotEmpty().WithMessage("RoleId is required");
        }
    }
}

using DotNetCleanTemplate.Domain.Common;
using DotNetCleanTemplate.Shared.DTOs;
using FluentValidation;

namespace DotNetCleanTemplate.Application.Validation
{
    public class UserWithRolesDtoValidator : AbstractValidator<UserWithRolesDto>
    {
        public UserWithRolesDtoValidator()
        {
            RuleFor(x => x.UserName)
                .NotEmpty()
                .MinimumLength(DomainConstants.MinUserNameLength)
                .MaximumLength(DomainConstants.MaxUserNameLength);

            RuleFor(x => x.Email)
                .NotEmpty()
                .MinimumLength(DomainConstants.MinEmailLength)
                .MaximumLength(DomainConstants.MaxEmailLength)
                .EmailAddress();

            RuleForEach(x => x.Roles).SetValidator(new RoleDtoValidator());
        }
    }

    public class RoleDtoValidator : AbstractValidator<RoleDto>
    {
        public RoleDtoValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty()
                .MinimumLength(DomainConstants.MinRoleNameLength)
                .MaximumLength(DomainConstants.MaxRoleNameLength);
        }
    }
}

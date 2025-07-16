using DotNetCleanTemplate.Domain.Common;
using DotNetCleanTemplate.Shared.DTOs;
using FluentValidation;

namespace DotNetCleanTemplate.Application.Validation
{
    public class RegisterUserDtoValidator : AbstractValidator<RegisterUserDto>
    {
        public RegisterUserDtoValidator()
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

            RuleFor(x => x.Password)
                .NotEmpty()
                .MinimumLength(DomainConstants.MinPasswordHashLength)
                .MaximumLength(DomainConstants.MaxPasswordHashLength);
        }
    }
}

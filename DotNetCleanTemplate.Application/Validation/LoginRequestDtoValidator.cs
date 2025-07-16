using DotNetCleanTemplate.Domain.Common;
using DotNetCleanTemplate.Shared.DTOs;
using FluentValidation;

namespace DotNetCleanTemplate.Application.Validation
{
    public class LoginRequestDtoValidator : AbstractValidator<LoginRequestDto>
    {
        public LoginRequestDtoValidator()
        {
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

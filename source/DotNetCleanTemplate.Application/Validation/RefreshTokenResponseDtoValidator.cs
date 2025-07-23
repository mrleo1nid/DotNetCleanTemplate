using DotNetCleanTemplate.Shared.DTOs;
using FluentValidation;

namespace DotNetCleanTemplate.Application.Validation
{
    public class RefreshTokenResponseDtoValidator : AbstractValidator<RefreshTokenResponseDto>
    {
        public RefreshTokenResponseDtoValidator()
        {
            RuleFor(x => x.AccessToken).NotEmpty();
            RuleFor(x => x.RefreshToken).NotEmpty();
        }
    }
}

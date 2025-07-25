using DotNetCleanTemplate.Shared.DTOs;
using FluentValidation;

namespace DotNetCleanTemplate.Application.Validation
{
    public class ChangeUserPasswordDtoValidator : AbstractValidator<ChangeUserPasswordDto>
    {
        public ChangeUserPasswordDtoValidator()
        {
            RuleFor(x => x.UserId).NotEmpty().WithMessage("ID пользователя обязателен");

            RuleFor(x => x.NewPassword)
                .NotEmpty()
                .WithMessage("Новый пароль обязателен")
                .MinimumLength(8)
                .WithMessage("Пароль должен содержать минимум 8 символов")
                .Matches(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]")
                .WithMessage(
                    "Пароль должен содержать хотя бы одну строчную букву, одну заглавную букву, одну цифру и один специальный символ"
                );

            RuleFor(x => x.ConfirmPassword)
                .NotEmpty()
                .WithMessage("Подтверждение пароля обязательно")
                .Equal(x => x.NewPassword)
                .WithMessage("Пароли не совпадают");
        }
    }
}

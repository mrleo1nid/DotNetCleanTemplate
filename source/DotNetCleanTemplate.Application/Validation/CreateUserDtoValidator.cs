using DotNetCleanTemplate.Domain.Common;
using DotNetCleanTemplate.Shared.DTOs;
using FluentValidation;

namespace DotNetCleanTemplate.Application.Validation;

public class CreateUserDtoValidator : AbstractValidator<CreateUserDto>
{
    public CreateUserDtoValidator()
    {
        RuleFor(x => x.UserName)
            .NotEmpty()
            .WithMessage("Имя пользователя обязательно")
            .MinimumLength(DomainConstants.MinUserNameLength)
            .MaximumLength(DomainConstants.MaxUserNameLength)
            .Matches("^[a-zA-Z0-9_-]+$")
            .WithMessage(
                "Имя пользователя может содержать только буквы, цифры, дефисы и подчеркивания"
            );

        RuleFor(x => x.Email)
            .NotEmpty()
            .WithMessage("Email обязателен")
            .MinimumLength(DomainConstants.MinEmailLength)
            .MaximumLength(DomainConstants.MaxEmailLength)
            .EmailAddress()
            .WithMessage("Некорректный формат email");

        RuleFor(x => x.Password)
            .NotEmpty()
            .WithMessage("Пароль обязателен")
            .MinimumLength(DomainConstants.MinPasswordHashLength)
            .MaximumLength(DomainConstants.MaxPasswordHashLength)
            .Matches("[A-Z]")
            .WithMessage("Пароль должен содержать хотя бы одну заглавную букву")
            .Matches("[a-z]")
            .WithMessage("Пароль должен содержать хотя бы одну строчную букву")
            .Matches("[0-9]")
            .WithMessage("Пароль должен содержать хотя бы одну цифру");

        RuleFor(x => x.ConfirmPassword)
            .NotEmpty()
            .WithMessage("Подтверждение пароля обязательно")
            .Equal(x => x.Password)
            .WithMessage("Пароли не совпадают");
    }
}

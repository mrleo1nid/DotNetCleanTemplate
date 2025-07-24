using DotNetCleanTemplate.Shared.DTOs;
using FluentValidation;

namespace DotNetCleanTemplate.Application.Validation
{
    public class CreateRoleDtoValidator : AbstractValidator<CreateRoleDto>
    {
        public CreateRoleDtoValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty()
                .WithMessage("Название роли обязательно")
                .MinimumLength(2)
                .WithMessage("Название роли должно содержать минимум 2 символа")
                .MaximumLength(50)
                .WithMessage("Название роли не должно превышать 50 символов")
                .Matches("^[a-zA-Zа-яА-Я0-9\\s]+$")
                .WithMessage("Название роли может содержать только буквы, цифры и пробелы");
        }
    }
}

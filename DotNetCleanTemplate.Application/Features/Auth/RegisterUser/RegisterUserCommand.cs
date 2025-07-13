using MediatR;
using DotNetCleanTemplate.Shared.DTOs;

namespace DotNetCleanTemplate.Application.Features.Auth.RegisterUser
{
    public record RegisterUserCommand : IRequest<Guid>
    {
        public RegisterUserDto Dto { get; init; } = null!;
    }
}

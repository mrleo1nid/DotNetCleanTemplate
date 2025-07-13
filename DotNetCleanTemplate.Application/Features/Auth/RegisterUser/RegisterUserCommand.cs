using DotNetCleanTemplate.Shared.DTOs;
using MediatR;

namespace DotNetCleanTemplate.Application.Features.Auth.RegisterUser
{
    public record RegisterUserCommand : IRequest<Guid>
    {
        public RegisterUserDto Dto { get; init; } = null!;
    }
}

using DotNetCleanTemplate.Shared.Common;
using DotNetCleanTemplate.Shared.DTOs;
using MediatR;

namespace DotNetCleanTemplate.Application.Features.Auth.RegisterUser
{
    public record RegisterUserCommand : IRequest<Result<Guid>>
    {
        public RegisterUserDto Dto { get; init; } = null!;
    }
}

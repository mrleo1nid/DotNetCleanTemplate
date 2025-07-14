using DotNetCleanTemplate.Application.Caching;
using DotNetCleanTemplate.Shared.Common;
using DotNetCleanTemplate.Shared.DTOs;
using MediatR;

namespace DotNetCleanTemplate.Application.Features.Auth.RegisterUser
{
    [InvalidateCache(region: "users")]
    public record RegisterUserCommand : IRequest<Result<Guid>>
    {
        public RegisterUserDto Dto { get; init; } = null!;
    }
}

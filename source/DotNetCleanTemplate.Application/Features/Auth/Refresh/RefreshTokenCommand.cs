using DotNetCleanTemplate.Shared.Common;
using DotNetCleanTemplate.Shared.DTOs;
using MediatR;

namespace DotNetCleanTemplate.Application.Features.Auth.Refresh
{
    public record RefreshTokenCommand(string RefreshToken)
        : IRequest<Result<RefreshTokenResponseDto>>;
}

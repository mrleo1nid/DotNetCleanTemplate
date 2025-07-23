using DotNetCleanTemplate.Shared.Common;
using DotNetCleanTemplate.Shared.DTOs;
using MediatR;

namespace DotNetCleanTemplate.Application.Features.Auth.Login
{
    public record LoginCommand(LoginRequestDto Dto) : IRequest<Result<LoginResponseDto>>;
}

using DotNetCleanTemplate.Shared.Common;
using DotNetCleanTemplate.Shared.DTOs;
using MediatR;

namespace DotNetCleanTemplate.Application.Features.Users
{
    public class ChangeUserPasswordCommand : IRequest<Result<Unit>>
    {
        public ChangeUserPasswordDto Dto { get; set; } = default!;
    }
}

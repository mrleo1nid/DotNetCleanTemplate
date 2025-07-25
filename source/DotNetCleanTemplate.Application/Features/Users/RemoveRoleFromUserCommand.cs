using DotNetCleanTemplate.Shared.Common;
using DotNetCleanTemplate.Shared.DTOs;
using MediatR;

namespace DotNetCleanTemplate.Application.Features.Users
{
    public class RemoveRoleFromUserCommand : IRequest<Result<Unit>>
    {
        public RemoveRoleFromUserDto Dto { get; set; } = default!;
    }
}

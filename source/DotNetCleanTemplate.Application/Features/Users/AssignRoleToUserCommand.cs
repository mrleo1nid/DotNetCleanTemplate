using DotNetCleanTemplate.Shared.Common;
using DotNetCleanTemplate.Shared.DTOs;
using MediatR;

namespace DotNetCleanTemplate.Application.Features.Users
{
    public class AssignRoleToUserCommand : IRequest<Result<Unit>>
    {
        public AssignRoleToUserDto Dto { get; set; } = null!;
    }
}

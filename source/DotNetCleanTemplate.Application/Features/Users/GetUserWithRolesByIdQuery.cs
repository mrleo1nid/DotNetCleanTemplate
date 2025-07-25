using DotNetCleanTemplate.Shared.Common;
using DotNetCleanTemplate.Shared.DTOs;
using MediatR;

namespace DotNetCleanTemplate.Application.Features.Users
{
    public class GetUserWithRolesByIdQuery : IRequest<Result<UserWithRolesDto>>
    {
        public Guid UserId { get; set; }
    }
}

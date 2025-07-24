using DotNetCleanTemplate.Shared.Common;
using MediatR;

namespace DotNetCleanTemplate.Application.Features.Users
{
    public class DeleteRoleCommand : IRequest<Result<Unit>>
    {
        public Guid RoleId { get; set; }
    }
}

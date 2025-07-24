using DotNetCleanTemplate.Shared.Common;
using DotNetCleanTemplate.Shared.DTOs;
using MediatR;

namespace DotNetCleanTemplate.Application.Features.Users
{
    public class CreateRoleCommand : IRequest<Result<RoleDto>>
    {
        public string Name { get; set; } = null!;
    }
}

using DotNetCleanTemplate.Application.Interfaces;
using DotNetCleanTemplate.Shared.Common;
using MediatR;

namespace DotNetCleanTemplate.Application.Features.Users
{
    public class DeleteRoleCommandHandler : IRequestHandler<DeleteRoleCommand, Result<Unit>>
    {
        private readonly IRoleService _roleService;

        public DeleteRoleCommandHandler(IRoleService roleService)
        {
            _roleService = roleService;
        }

        public async Task<Result<Unit>> Handle(
            DeleteRoleCommand request,
            CancellationToken cancellationToken
        )
        {
            var result = await _roleService.DeleteRoleAsync(request.RoleId, cancellationToken);
            return result;
        }
    }
}

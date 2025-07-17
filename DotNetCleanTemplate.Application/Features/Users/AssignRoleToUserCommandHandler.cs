using DotNetCleanTemplate.Application.Interfaces;
using DotNetCleanTemplate.Shared.Common;
using MediatR;

namespace DotNetCleanTemplate.Application.Features.Users
{
    public class AssignRoleToUserCommandHandler
        : IRequestHandler<AssignRoleToUserCommand, Result<Unit>>
    {
        private readonly IUserService _userService;

        public AssignRoleToUserCommandHandler(IUserService userService)
        {
            _userService = userService;
        }

        public async Task<Result<Unit>> Handle(
            AssignRoleToUserCommand request,
            CancellationToken cancellationToken
        )
        {
            return await _userService.AssignRoleToUserAsync(
                request.Dto.UserId,
                request.Dto.RoleId,
                cancellationToken
            );
        }
    }
}

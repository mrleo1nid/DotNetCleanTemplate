using DotNetCleanTemplate.Application.Interfaces;
using DotNetCleanTemplate.Shared.Common;
using MediatR;

namespace DotNetCleanTemplate.Application.Features.Users
{
    public class RemoveRoleFromUserCommandHandler
        : IRequestHandler<RemoveRoleFromUserCommand, Result<Unit>>
    {
        private readonly IUserService _userService;

        public RemoveRoleFromUserCommandHandler(IUserService userService)
        {
            _userService = userService;
        }

        public async Task<Result<Unit>> Handle(
            RemoveRoleFromUserCommand request,
            CancellationToken cancellationToken
        )
        {
            return await _userService.RemoveRoleFromUserAsync(
                request.Dto.UserId,
                request.Dto.RoleId,
                cancellationToken
            );
        }
    }
}

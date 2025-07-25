using DotNetCleanTemplate.Application.Interfaces;
using DotNetCleanTemplate.Shared.Common;
using MediatR;

namespace DotNetCleanTemplate.Application.Features.Users
{
    public class DeleteUserCommandHandler : IRequestHandler<DeleteUserCommand, Result<Unit>>
    {
        private readonly IUserService _userService;

        public DeleteUserCommandHandler(IUserService userService)
        {
            _userService = userService;
        }

        public async Task<Result<Unit>> Handle(
            DeleteUserCommand request,
            CancellationToken cancellationToken
        )
        {
            var result = await _userService.DeleteUserAsync(request.UserId, cancellationToken);
            return result;
        }
    }
}

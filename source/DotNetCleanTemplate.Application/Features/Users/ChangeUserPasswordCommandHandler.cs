using DotNetCleanTemplate.Application.Interfaces;
using DotNetCleanTemplate.Shared.Common;
using MediatR;

namespace DotNetCleanTemplate.Application.Features.Users
{
    public class ChangeUserPasswordCommandHandler
        : IRequestHandler<ChangeUserPasswordCommand, Result<Unit>>
    {
        private readonly IUserService _userService;

        public ChangeUserPasswordCommandHandler(IUserService userService)
        {
            _userService = userService;
        }

        public async Task<Result<Unit>> Handle(
            ChangeUserPasswordCommand request,
            CancellationToken cancellationToken
        )
        {
            var result = await _userService.ChangeUserPasswordAsync(
                request.Dto.UserId,
                request.Dto.NewPassword,
                cancellationToken
            );
            return result;
        }
    }
}

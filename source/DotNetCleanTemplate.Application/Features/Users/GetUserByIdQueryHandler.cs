using DotNetCleanTemplate.Application.Interfaces;
using DotNetCleanTemplate.Domain.Entities;
using DotNetCleanTemplate.Shared.Common;
using MediatR;

namespace DotNetCleanTemplate.Application.Features.Users
{
    public class GetUserByIdQueryHandler : IRequestHandler<GetUserByIdQuery, Result<User>>
    {
        private readonly IUserService _userService;

        public GetUserByIdQueryHandler(IUserService userService)
        {
            _userService = userService;
        }

        public async Task<Result<User>> Handle(
            GetUserByIdQuery request,
            CancellationToken cancellationToken
        )
        {
            // Получаем всех пользователей с ролями и находим нужного
            var allUsersResult = await _userService.GetAllUsersWithRolesAsync(cancellationToken);

            if (!allUsersResult.IsSuccess)
            {
                return Result<User>.Failure(allUsersResult.Errors);
            }

            var user = allUsersResult.Value.FirstOrDefault(u => u.Id == request.UserId);

            if (user == null)
            {
                return Result<User>.Failure(ErrorCodes.UserNotFound, "Пользователь не найден");
            }

            return Result<User>.Success(user);
        }
    }
}

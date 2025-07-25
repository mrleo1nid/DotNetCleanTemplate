using DotNetCleanTemplate.Application.Interfaces;
using DotNetCleanTemplate.Application.Mapping;
using DotNetCleanTemplate.Shared.Common;
using DotNetCleanTemplate.Shared.DTOs;
using Mapster;
using MediatR;

namespace DotNetCleanTemplate.Application.Features.Users
{
    public class GetUserWithRolesByIdQueryHandler
        : IRequestHandler<GetUserWithRolesByIdQuery, Result<UserWithRolesDto>>
    {
        private readonly IUserService _userService;

        public GetUserWithRolesByIdQueryHandler(IUserService userService)
        {
            _userService = userService;
        }

        public async Task<Result<UserWithRolesDto>> Handle(
            GetUserWithRolesByIdQuery request,
            CancellationToken cancellationToken
        )
        {
            // Получаем всех пользователей с ролями и находим нужного
            var allUsersResult = await _userService.GetAllUsersWithRolesAsync(cancellationToken);

            if (!allUsersResult.IsSuccess)
            {
                return Result<UserWithRolesDto>.Failure(allUsersResult.Errors);
            }

            var user = allUsersResult.Value.FirstOrDefault(u => u.Id == request.UserId);

            if (user == null)
            {
                return Result<UserWithRolesDto>.Failure(
                    ErrorCodes.UserNotFound,
                    "Пользователь не найден"
                );
            }

            // Маппим в DTO
            var userDto = user.Adapt<UserWithRolesDto>();

            return Result<UserWithRolesDto>.Success(userDto);
        }
    }
}

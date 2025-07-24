using DotNetCleanTemplate.Application.Interfaces;
using DotNetCleanTemplate.Shared.Common;
using DotNetCleanTemplate.Shared.DTOs;
using MapsterMapper;
using MediatR;

namespace DotNetCleanTemplate.Application.Features.Users
{
    public class GetUsersWithRolesPaginatedQueryHandler
        : IRequestHandler<
            GetUsersWithRolesPaginatedQuery,
            Result<PaginatedResultDto<UserWithRolesDto>>
        >
    {
        private readonly IUserService _userService;
        private readonly IMapper _mapper;

        public GetUsersWithRolesPaginatedQueryHandler(IUserService userService, IMapper mapper)
        {
            _userService = userService;
            _mapper = mapper;
        }

        public async Task<Result<PaginatedResultDto<UserWithRolesDto>>> Handle(
            GetUsersWithRolesPaginatedQuery request,
            CancellationToken cancellationToken
        )
        {
            var result = await _userService.GetUsersWithRolesPaginatedAsync(
                request.Page,
                request.PageSize,
                cancellationToken
            );

            if (!result.IsSuccess)
                return Result<PaginatedResultDto<UserWithRolesDto>>.Failure(result.Errors);

            var (users, totalCount) = result.Value;
            var mappedUsers = _mapper.Map<List<UserWithRolesDto>>(users);

            var totalPages = (int)Math.Ceiling((double)totalCount / request.PageSize);
            var hasPreviousPage = request.Page > 1;
            var hasNextPage = request.Page < totalPages;

            var paginatedResult = new PaginatedResultDto<UserWithRolesDto>
            {
                Items = mappedUsers,
                TotalCount = totalCount,
                Page = request.Page,
                PageSize = request.PageSize,
                TotalPages = totalPages,
                HasPreviousPage = hasPreviousPage,
                HasNextPage = hasNextPage,
            };

            return Result<PaginatedResultDto<UserWithRolesDto>>.Success(paginatedResult);
        }
    }
}

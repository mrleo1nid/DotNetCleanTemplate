using DotNetCleanTemplate.Application.Interfaces;
using DotNetCleanTemplate.Shared.Common;
using DotNetCleanTemplate.Shared.DTOs;
using Mapster;
using MediatR;

namespace DotNetCleanTemplate.Application.Features.Users
{
    public class GetRolesPaginatedQueryHandler
        : IRequestHandler<GetRolesPaginatedQuery, Result<PaginatedResultDto<RoleDto>>>
    {
        private readonly IRoleService _roleService;

        public GetRolesPaginatedQueryHandler(IRoleService roleService)
        {
            _roleService = roleService;
        }

        public async Task<Result<PaginatedResultDto<RoleDto>>> Handle(
            GetRolesPaginatedQuery request,
            CancellationToken cancellationToken
        )
        {
            var result = await _roleService.GetRolesPaginatedAsync(
                request.Page,
                request.PageSize,
                cancellationToken
            );

            if (!result.IsSuccess)
                return Result<PaginatedResultDto<RoleDto>>.Failure(result.Errors[0]);

            var (roles, totalCount) = result.Value;
            var mappedRoles = roles.Adapt<List<RoleDto>>();

            var totalPages = (int)Math.Ceiling((double)totalCount / request.PageSize);
            var hasPreviousPage = request.Page > 1;
            var hasNextPage = request.Page < totalPages;

            var paginatedResult = new PaginatedResultDto<RoleDto>
            {
                Items = mappedRoles,
                TotalCount = totalCount,
                Page = request.Page,
                PageSize = request.PageSize,
                TotalPages = totalPages,
                HasPreviousPage = hasPreviousPage,
                HasNextPage = hasNextPage,
            };

            return Result<PaginatedResultDto<RoleDto>>.Success(paginatedResult);
        }
    }
}

using DotNetCleanTemplate.Application.Caching;
using DotNetCleanTemplate.Shared.Common;
using DotNetCleanTemplate.Shared.DTOs;
using MediatR;

namespace DotNetCleanTemplate.Application.Features.Users
{
    [Cache("users_paginated", "users")]
    public class GetUsersWithRolesPaginatedQuery
        : IRequest<Result<PaginatedResultDto<UserWithRolesDto>>>
    {
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }
}

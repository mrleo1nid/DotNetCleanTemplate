using DotNetCleanTemplate.Application.Caching;
using DotNetCleanTemplate.Shared.Common;
using DotNetCleanTemplate.Shared.DTOs;
using MediatR;

namespace DotNetCleanTemplate.Application.Features.Users
{
    [Cache("users_all", "users")]
    public class GetAllUsersWithRolesQuery : IRequest<Result<List<UserWithRolesDto>>> { }
}

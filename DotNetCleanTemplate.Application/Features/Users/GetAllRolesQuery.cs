using System.Collections.Generic;
using DotNetCleanTemplate.Shared.Common;
using DotNetCleanTemplate.Shared.DTOs;
using MediatR;

namespace DotNetCleanTemplate.Application.Features.Users
{
    public class GetAllRolesQuery : IRequest<Result<List<RoleDto>>> { }
}

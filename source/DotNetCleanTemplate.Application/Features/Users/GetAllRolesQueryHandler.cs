using DotNetCleanTemplate.Application.Interfaces;
using DotNetCleanTemplate.Shared.Common;
using DotNetCleanTemplate.Shared.DTOs;
using Mapster;
using MediatR;

namespace DotNetCleanTemplate.Application.Features.Users
{
    public class GetAllRolesQueryHandler : IRequestHandler<GetAllRolesQuery, Result<List<RoleDto>>>
    {
        private readonly IRoleService _roleService;

        public GetAllRolesQueryHandler(IRoleService roleService)
        {
            _roleService = roleService;
        }

        public async Task<Result<List<RoleDto>>> Handle(
            GetAllRolesQuery request,
            CancellationToken cancellationToken
        )
        {
            var result = await _roleService.GetAllRolesAsync(cancellationToken);
            if (!result.IsSuccess)
                return Result<List<RoleDto>>.Failure(result.Errors[0]);
            var dtos = result.Value!.Adapt<List<RoleDto>>();
            return Result<List<RoleDto>>.Success(dtos);
        }
    }
}

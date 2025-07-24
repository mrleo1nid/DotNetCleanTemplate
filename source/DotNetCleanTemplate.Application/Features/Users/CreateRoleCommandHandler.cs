using DotNetCleanTemplate.Application.Interfaces;
using DotNetCleanTemplate.Domain.Entities;
using DotNetCleanTemplate.Domain.ValueObjects.Role;
using DotNetCleanTemplate.Shared.Common;
using DotNetCleanTemplate.Shared.DTOs;
using MediatR;

namespace DotNetCleanTemplate.Application.Features.Users
{
    public class CreateRoleCommandHandler : IRequestHandler<CreateRoleCommand, Result<RoleDto>>
    {
        private readonly IRoleService _roleService;

        public CreateRoleCommandHandler(IRoleService roleService)
        {
            _roleService = roleService;
        }

        public async Task<Result<RoleDto>> Handle(
            CreateRoleCommand request,
            CancellationToken cancellationToken
        )
        {
            // Проверяем, существует ли роль с таким именем
            var existingRole = await _roleService.FindByNameAsync(request.Name, cancellationToken);
            if (existingRole.IsSuccess)
            {
                return Result<RoleDto>.Failure(
                    "Role.AlreadyExists",
                    $"Role with name '{request.Name}' already exists."
                );
            }

            // Создаем новую роль
            RoleName roleName;
            try
            {
                roleName = new RoleName(request.Name);
            }
            catch (ArgumentException ex)
            {
                return Result<RoleDto>.Failure("Role.InvalidName", ex.Message);
            }

            var role = new Role(roleName);
            var result = await _roleService.CreateRoleAsync(role, cancellationToken);

            if (!result.IsSuccess)
            {
                return Result<RoleDto>.Failure(result.Errors[0]);
            }

            var roleDto = new RoleDto { Id = result.Value.Id, Name = result.Value.Name.Value };

            return Result<RoleDto>.Success(roleDto);
        }
    }
}

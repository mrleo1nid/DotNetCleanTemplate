using DotNetCleanTemplate.Application.Configurations;
using DotNetCleanTemplate.Application.Interfaces;
using DotNetCleanTemplate.Domain.Entities;
using DotNetCleanTemplate.Domain.Repositories;
using DotNetCleanTemplate.Shared.Common;
using MediatR;
using Microsoft.Extensions.Options;

namespace DotNetCleanTemplate.Application.Services
{
    public class RoleService : IRoleService
    {
        private readonly IRoleRepository _roleRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IDefaultRoleService _defaultRoleService;

        public RoleService(
            IRoleRepository roleRepository,
            IUnitOfWork unitOfWork,
            IOptions<DefaultSettings> defaultSettings,
            IDefaultRoleService defaultRoleService
        )
        {
            _roleRepository = roleRepository;
            _unitOfWork = unitOfWork;
            _defaultRoleService = defaultRoleService;
        }

        public async Task<Result<Role>> FindByNameAsync(
            string name,
            CancellationToken cancellationToken = default
        )
        {
            var role = await _roleRepository.FindByNameAsync(name, cancellationToken);
            if (role is null)
                return Result<Role>.Failure("Role.NotFound", $"Role with name '{name}' not found.");
            return Result<Role>.Success(role);
        }

        public async Task<Result<Role>> CreateRoleAsync(
            Role role,
            CancellationToken cancellationToken = default
        )
        {
            var createdRole = await _roleRepository.AddAsync(role);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return Result<Role>.Success(createdRole);
        }

        public async Task<Result<Unit>> DeleteRoleAsync(
            Guid roleId,
            CancellationToken cancellationToken = default
        )
        {
            var role = await _roleRepository.GetByIdAsync<Role>(roleId);
            if (role is null)
                return Result<Unit>.Failure("Role.NotFound", $"Role with ID '{roleId}' not found.");

            // Проверяем, не является ли роль дефолтной
            if (_defaultRoleService.IsDefaultRole(role.Name.Value))
            {
                return Result<Unit>.Failure(
                    "Role.CannotDeleteDefault",
                    $"Cannot delete default role '{role.Name.Value}'. Default roles are protected from deletion."
                );
            }

            await _roleRepository.DeleteAsync(role);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return Result<Unit>.Success(Unit.Value);
        }

        public async Task<Result<List<Role>>> GetAllRolesAsync(
            CancellationToken cancellationToken = default
        )
        {
            var roles = await _roleRepository.GetAllAsync<Role>();
            return Result<List<Role>>.Success(roles.ToList());
        }
    }
}

using DotNetCleanTemplate.Application.Interfaces;
using DotNetCleanTemplate.Domain.Entities;
using DotNetCleanTemplate.Domain.Repositories;
using DotNetCleanTemplate.Shared.Common;

namespace DotNetCleanTemplate.Application.Services
{
    public class RoleService : IRoleService
    {
        private readonly IRoleRepository _roleRepository;
        private readonly IUnitOfWork _unitOfWork;

        public RoleService(IRoleRepository roleRepository, IUnitOfWork unitOfWork)
        {
            _roleRepository = roleRepository;
            _unitOfWork = unitOfWork;
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

        public async Task<Result<List<Role>>> GetAllRolesAsync(
            CancellationToken cancellationToken = default
        )
        {
            var roles = await _roleRepository.GetAllAsync<Role>();
            return Result<List<Role>>.Success(roles.ToList());
        }
    }
}

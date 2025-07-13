using System.Threading;
using System.Threading.Tasks;
using DotNetCleanTemplate.Domain.Entities;
using DotNetCleanTemplate.Domain.Repositories;

namespace DotNetCleanTemplate.Application.Services
{
    public class RoleService
    {
        private readonly IRoleRepository _roleRepository;
        private readonly IUnitOfWork _unitOfWork;

        public RoleService(IRoleRepository roleRepository, IUnitOfWork unitOfWork)
        {
            _roleRepository = roleRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<Role?> FindByNameAsync(
            string name,
            CancellationToken cancellationToken = default
        )
        {
            return await _roleRepository.FindByNameAsync(name, cancellationToken);
        }

        public async Task<Role> CreateRoleAsync(
            Role role,
            CancellationToken cancellationToken = default
        )
        {
            var createdRole = await _roleRepository.AddAsync(role);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return createdRole;
        }
    }
}

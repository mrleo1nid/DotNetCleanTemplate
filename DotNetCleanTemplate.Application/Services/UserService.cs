using System.Threading;
using System.Threading.Tasks;
using DotNetCleanTemplate.Domain.Entities;
using DotNetCleanTemplate.Domain.Repositories;

namespace DotNetCleanTemplate.Application.Services
{
    public class UserService
    {
        private readonly IUserRepository _userRepository;
        private readonly IUnitOfWork _unitOfWork;

        public UserService(IUserRepository userRepository, IUnitOfWork unitOfWork)
        {
            _userRepository = userRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<User?> FindByEmailAsync(
            string email,
            CancellationToken cancellationToken = default
        )
        {
            return await _userRepository.FindByEmailAsync(email, cancellationToken);
        }

        public async Task<User> CreateUserAsync(
            User user,
            CancellationToken cancellationToken = default
        )
        {
            var createdUser = await _userRepository.AddAsync(user);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return createdUser;
        }
    }
}

using DotNetCleanTemplate.Domain.Entities;
using DotNetCleanTemplate.Domain.Repositories;
using DotNetCleanTemplate.Shared.Common;

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

        public async Task<Result<User>> FindByEmailAsync(
            string email,
            CancellationToken cancellationToken = default
        )
        {
            var user = await _userRepository.FindByEmailAsync(email, cancellationToken);
            if (user is null)
                return Result<User>.Failure(
                    "User.NotFound",
                    $"User with email '{email}' not found."
                );
            return Result<User>.Success(user);
        }

        public async Task<Result<User>> CreateUserAsync(
            User user,
            CancellationToken cancellationToken = default
        )
        {
            var existing = await _userRepository.FindByEmailAsync(
                user.Email.Value,
                cancellationToken
            );
            if (existing != null)
                return Result<User>.Failure(
                    "User.AlreadyExists",
                    $"User with email '{user.Email.Value}' already exists."
                );
            var createdUser = await _userRepository.AddAsync(user);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return Result<User>.Success(createdUser);
        }
    }
}

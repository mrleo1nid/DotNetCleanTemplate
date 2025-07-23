using DotNetCleanTemplate.Application.Interfaces;
using DotNetCleanTemplate.Domain.Entities;
using DotNetCleanTemplate.Domain.Repositories;
using DotNetCleanTemplate.Shared.Common;
using MediatR;

namespace DotNetCleanTemplate.Application.Services
{
    public class UserService : IUserService
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

        public async Task<Result<List<User>>> GetAllUsersWithRolesAsync(
            CancellationToken cancellationToken = default
        )
        {
            var users = await _userRepository.GetAllUsersWithRolesAsync(cancellationToken);
            if (users == null || users.Count == 0)
                return Result<List<User>>.Failure("User.NotFound", "Пользователи не найдены.");
            return Result<List<User>>.Success(users);
        }

        public async Task<Result<Unit>> AssignRoleToUserAsync(
            Guid userId,
            Guid roleId,
            CancellationToken cancellationToken = default
        )
        {
            // Получаем пользователя
            var user = await _userRepository.GetByIdAsync<User>(userId);
            if (user == null)
                return Result<Unit>.Failure("User.NotFound", $"User with id '{userId}' not found.");

            // Получаем роль
            var role = await _userRepository.GetByIdAsync<Role>(roleId);
            if (role == null)
                return Result<Unit>.Failure("Role.NotFound", $"Role with id '{roleId}' not found.");

            // Проверяем, есть ли уже такая роль у пользователя
            if (user.UserRoles.Any(ur => ur.RoleId == roleId))
                return Result<Unit>.Failure(
                    "UserRole.AlreadyExists",
                    "User already has this role."
                );

            // Добавляем роль пользователю через доменный метод
            user.AssignRole(role);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return Result<Unit>.Success();
        }
    }
}

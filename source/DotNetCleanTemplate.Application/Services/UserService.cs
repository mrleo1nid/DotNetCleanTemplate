using DotNetCleanTemplate.Application.Configurations;
using DotNetCleanTemplate.Application.Interfaces;
using DotNetCleanTemplate.Domain.Entities;
using DotNetCleanTemplate.Domain.Repositories;
using DotNetCleanTemplate.Domain.Services;
using DotNetCleanTemplate.Domain.ValueObjects.User;
using DotNetCleanTemplate.Shared.Common;
using MediatR;
using Microsoft.Extensions.Options;

namespace DotNetCleanTemplate.Application.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly IRoleRepository _roleRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IPasswordHasher _passwordHasher;
        private readonly DefaultSettings _defaultSettings;

        public UserService(
            IUserRepository userRepository,
            IRoleRepository roleRepository,
            IUnitOfWork unitOfWork,
            IPasswordHasher passwordHasher,
            IOptions<DefaultSettings> defaultSettings
        )
        {
            _userRepository = userRepository;
            _roleRepository = roleRepository;
            _unitOfWork = unitOfWork;
            _passwordHasher = passwordHasher;
            _defaultSettings = defaultSettings.Value;
        }

        public async Task<Result<User>> FindByEmailAsync(
            string email,
            CancellationToken cancellationToken = default
        )
        {
            var user = await _userRepository.FindByEmailAsync(email, cancellationToken);
            if (user is null)
                return Result<User>.Failure(
                    ErrorCodes.UserNotFound,
                    $"User with email '{email}' not found."
                );
            return Result<User>.Success(user);
        }

        public async Task<Result<User>> CreateUserAsync(
            User user,
            CancellationToken cancellationToken = default
        )
        {
            var existingByEmail = await _userRepository.FindByEmailAsync(
                user.Email.Value,
                cancellationToken
            );
            if (existingByEmail != null)
                return Result<User>.Failure(
                    ErrorCodes.UserAlreadyExists,
                    $"User with email '{user.Email.Value}' already exists."
                );

            var existingByUserName = await _userRepository.FindByUserNameAsync(
                user.Name.Value,
                cancellationToken
            );
            if (existingByUserName != null)
                return Result<User>.Failure(
                    ErrorCodes.UserAlreadyExists,
                    $"User with username '{user.Name.Value}' already exists."
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
                return Result<List<User>>.Failure(
                    ErrorCodes.UserNotFound,
                    "Пользователи не найдены."
                );
            return Result<List<User>>.Success(users);
        }

        public async Task<
            Result<(List<User> Users, int TotalCount)>
        > GetUsersWithRolesPaginatedAsync(
            int page,
            int pageSize,
            CancellationToken cancellationToken = default
        )
        {
            var (users, totalCount) = await _userRepository.GetUsersWithRolesPaginatedAsync(
                page,
                pageSize,
                cancellationToken
            );
            if (users == null || users.Count == 0)
                return Result<(List<User> Users, int TotalCount)>.Failure(
                    ErrorCodes.UserNotFound,
                    "Пользователи не найдены."
                );
            return Result<(List<User> Users, int TotalCount)>.Success((users, totalCount));
        }

        public async Task<Result<Unit>> AssignRoleToUserAsync(
            Guid userId,
            Guid roleId,
            CancellationToken cancellationToken = default
        )
        {
            // Получаем пользователя с ролями
            var user = await _userRepository.GetUserWithRolesAsync(userId, cancellationToken);
            if (user == null)
                return Result<Unit>.Failure(
                    ErrorCodes.UserNotFound,
                    $"User with id '{userId}' not found."
                );

            // Получаем роль
            var role = await _roleRepository.GetByIdAsync(roleId);
            if (role == null)
                return Result<Unit>.Failure(
                    ErrorCodes.RoleNotFound,
                    $"Role with id '{roleId}' not found."
                );

            // Проверяем, есть ли уже такая роль у пользователя
            if (user.UserRoles.Any(ur => ur.RoleId == roleId))
                return Result<Unit>.Failure(
                    ErrorCodes.UserRoleAlreadyExists,
                    "User already has this role."
                );

            // Добавляем роль пользователю через доменный метод
            user.AssignRole(role);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return Result<Unit>.Success();
        }

        public async Task<Result<Unit>> DeleteUserAsync(
            Guid userId,
            CancellationToken cancellationToken = default
        )
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
                return Result<Unit>.Failure(
                    ErrorCodes.UserNotFound,
                    $"User with id '{userId}' not found."
                );

            await _userRepository.DeleteAsync(user);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return Result<Unit>.Success();
        }

        public async Task<Result<Unit>> ChangeUserPasswordAsync(
            Guid userId,
            string newPassword,
            CancellationToken cancellationToken = default
        )
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
                return Result<Unit>.Failure(
                    ErrorCodes.UserNotFound,
                    $"User with id '{userId}' not found."
                );

            // Хешируем новый пароль
            var newPasswordHash = new PasswordHash(_passwordHasher.HashPassword(newPassword));

            // Обновляем пароль пользователя
            user.ChangePassword(newPasswordHash);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return Result<Unit>.Success();
        }

        public async Task<Result<Unit>> RemoveRoleFromUserAsync(
            Guid userId,
            Guid roleId,
            CancellationToken cancellationToken = default
        )
        {
            // Получаем пользователя с ролями
            var user = await _userRepository.GetUserWithRolesAsync(userId, cancellationToken);
            if (user == null)
                return Result<Unit>.Failure(
                    ErrorCodes.UserNotFound,
                    $"User with id '{userId}' not found."
                );

            // Получаем роль
            var role = await _roleRepository.GetByIdAsync(roleId);
            if (role == null)
                return Result<Unit>.Failure(
                    ErrorCodes.RoleNotFound,
                    $"Role with id '{roleId}' not found."
                );

            // Проверяем, есть ли такая роль у пользователя
            if (!user.UserRoles.Any(ur => ur.RoleId == roleId))
                return Result<Unit>.Failure(
                    ErrorCodes.UserRoleNotFound,
                    "User does not have this role."
                );

            // Проверяем, является ли роль ролью админа
            if (role.Name.Value == _defaultSettings.DefaultAdminRole)
            {
                // Проверяем, сколько пользователей имеют роль админа
                var usersWithAdminRole = await _userRepository.GetUsersByRoleAsync(
                    role.Id,
                    cancellationToken
                );

                // Если только один пользователь имеет роль админа, не позволяем удалить её
                if (usersWithAdminRole.Count() <= 1)
                    return Result<Unit>.Failure(
                        ErrorCodes.CannotRemoveLastAdmin,
                        "Cannot remove the last admin role."
                    );
            }

            // Удаляем роль у пользователя через доменный метод
            user.RemoveRole(role);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return Result<Unit>.Success();
        }
    }
}

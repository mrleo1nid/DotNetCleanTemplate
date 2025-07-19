using DotNetCleanTemplate.Application.Configurations;
using DotNetCleanTemplate.Application.Interfaces;
using DotNetCleanTemplate.Domain.Repositories;
using DotNetCleanTemplate.Shared.Common;
using MediatR;
using Microsoft.Extensions.Options;

namespace DotNetCleanTemplate.Application.Services;

public class UserLockoutService : IUserLockoutService
{
    private readonly IUserLockoutRepository _userLockoutRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly FailToBanSettings _settings;

    public UserLockoutService(
        IUserLockoutRepository userLockoutRepository,
        IUnitOfWork unitOfWork,
        IOptions<FailToBanSettings> settings
    )
    {
        _userLockoutRepository = userLockoutRepository;
        _unitOfWork = unitOfWork;
        _settings = settings.Value;
    }

    public async Task<Result<Unit>> CheckUserLockoutAsync(
        Guid userId,
        CancellationToken cancellationToken = default
    )
    {
        if (!_settings.EnableFailToBan)
            return Result<Unit>.Success();

        var isLocked = await _userLockoutRepository.IsUserLockedAsync(userId, cancellationToken);

        if (isLocked)
        {
            return Result<Unit>.Failure(
                "Auth.UserLocked",
                "Account is temporarily locked due to multiple failed login attempts. Please try again later."
            );
        }

        return Result<Unit>.Success();
    }

    public async Task<Result<Unit>> RecordFailedLoginAttemptAsync(
        Guid userId,
        CancellationToken cancellationToken = default
    )
    {
        if (!_settings.EnableFailToBan)
            return Result<Unit>.Success();

        var lockout = await _userLockoutRepository.GetByUserIdAsync(userId, cancellationToken);

        if (lockout == null)
        {
            // Создаем новую запись блокировки
            lockout = new Domain.Entities.UserLockout(
                userId,
                DateTime.UtcNow.AddMinutes(_settings.LockoutDurationMinutes),
                1
            );
            await _userLockoutRepository.AddAsync(lockout);
        }
        else
        {
            // Увеличиваем счетчик неудачных попыток
            lockout.IncrementFailedAttempts();

            // Если достигли максимального количества попыток, продлеваем блокировку
            if (lockout.FailedAttempts >= _settings.MaxFailedAttempts)
            {
                lockout.ExtendLockout(DateTime.UtcNow.AddMinutes(_settings.LockoutDurationMinutes));
            }
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Result<Unit>.Success();
    }

    public async Task<Result<Unit>> RecordSuccessfulLoginAsync(
        Guid userId,
        CancellationToken cancellationToken = default
    )
    {
        if (!_settings.EnableFailToBan)
            return Result<Unit>.Success();

        var lockout = await _userLockoutRepository.GetByUserIdAsync(userId, cancellationToken);

        if (lockout != null)
        {
            lockout.ClearLockout();
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }

        return Result<Unit>.Success();
    }

    public async Task<Result<Unit>> ClearUserLockoutAsync(
        Guid userId,
        CancellationToken cancellationToken = default
    )
    {
        var lockout = await _userLockoutRepository.GetByUserIdAsync(userId, cancellationToken);

        if (lockout != null)
        {
            lockout.ClearLockout();
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }

        return Result<Unit>.Success();
    }
}

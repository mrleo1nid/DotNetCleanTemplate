using DotNetCleanTemplate.Application.Configurations;
using DotNetCleanTemplate.Application.Interfaces;
using DotNetCleanTemplate.Domain.Repositories;
using DotNetCleanTemplate.Shared.Common;
using MediatR;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace DotNetCleanTemplate.Application.Services;

public class UserLockoutService : IUserLockoutService
{
    private readonly IUserLockoutRepository _userLockoutRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly FailToBanSettings _settings;
    private readonly ILogger<UserLockoutService> _logger;

    public UserLockoutService(
        IUserLockoutRepository userLockoutRepository,
        IUnitOfWork unitOfWork,
        IOptions<FailToBanSettings> settings,
        ILogger<UserLockoutService> logger
    )
    {
        _userLockoutRepository = userLockoutRepository;
        _unitOfWork = unitOfWork;
        _settings = settings.Value;
        _logger = logger;
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

        try
        {
            // Используем атомарную операцию для увеличения счетчика
            await _userLockoutRepository.IncrementFailedAttemptsAsync(
                userId,
                _settings.MaxFailedAttempts,
                _settings.LockoutDurationMinutes,
                cancellationToken
            );

            // Сохраняем изменения в базе данных
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Recorded failed login attempt for user {UserId}", userId);
            return Result<Unit>.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error recording failed login attempt for user {UserId}", userId);
            return Result<Unit>.Failure("Lockout.Error", "Failed to record login attempt");
        }
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

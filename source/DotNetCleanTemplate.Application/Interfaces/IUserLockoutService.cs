using DotNetCleanTemplate.Shared.Common;
using MediatR;

namespace DotNetCleanTemplate.Application.Interfaces;

public interface IUserLockoutService
{
    Task<Result<Unit>> CheckUserLockoutAsync(
        Guid userId,
        CancellationToken cancellationToken = default
    );
    Task<Result<Unit>> RecordFailedLoginAttemptAsync(
        Guid userId,
        CancellationToken cancellationToken = default
    );
    Task<Result<Unit>> RecordSuccessfulLoginAsync(
        Guid userId,
        CancellationToken cancellationToken = default
    );
    Task<Result<Unit>> ClearUserLockoutAsync(
        Guid userId,
        CancellationToken cancellationToken = default
    );
}

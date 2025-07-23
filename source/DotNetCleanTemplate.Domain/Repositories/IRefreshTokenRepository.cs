using DotNetCleanTemplate.Domain.Entities;

namespace DotNetCleanTemplate.Domain.Repositories
{
    public interface IRefreshTokenRepository : IRepository
    {
        Task<RefreshToken?> FindByTokenAsync(
            string token,
            CancellationToken cancellationToken = default
        );
        Task<List<RefreshToken>> GetActiveTokensByUserIdAsync(
            Guid userId,
            CancellationToken cancellationToken = default
        );
        Task<List<RefreshToken>> GetExpiredTokensAsync(
            CancellationToken cancellationToken = default
        );
    }
}

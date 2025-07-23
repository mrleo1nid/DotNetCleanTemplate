using DotNetCleanTemplate.Domain.Entities;
using DotNetCleanTemplate.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace DotNetCleanTemplate.Infrastructure.Persistent.Repositories
{
    public class RefreshTokenRepository : BaseRepository, IRefreshTokenRepository
    {
        public RefreshTokenRepository(AppDbContext context)
            : base(context) { }

        public async Task<RefreshToken?> FindByTokenAsync(
            string token,
            CancellationToken cancellationToken = default
        )
        {
            return await _context.RefreshTokens.FirstOrDefaultAsync(
                rt => rt.Token == token,
                cancellationToken
            );
        }

        public async Task<List<RefreshToken>> GetActiveTokensByUserIdAsync(
            Guid userId,
            CancellationToken cancellationToken = default
        )
        {
            var now = DateTime.UtcNow;
            return await _context
                .RefreshTokens.Where(rt =>
                    rt.UserId == userId && rt.RevokedAt == null && rt.Expires > now
                )
                .ToListAsync(cancellationToken);
        }

        public async Task<List<RefreshToken>> GetExpiredTokensAsync(
            CancellationToken cancellationToken = default
        )
        {
            var now = DateTime.UtcNow;
            return await _context
                .RefreshTokens.Where(rt => rt.Expires <= now)
                .ToListAsync(cancellationToken);
        }
    }
}

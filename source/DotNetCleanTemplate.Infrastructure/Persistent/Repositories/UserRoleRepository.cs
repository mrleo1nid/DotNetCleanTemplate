using DotNetCleanTemplate.Domain.Entities;
using DotNetCleanTemplate.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace DotNetCleanTemplate.Infrastructure.Persistent.Repositories
{
    public class UserRoleRepository : BaseRepository<UserRole>, IUserRoleRepository
    {
        public UserRoleRepository(AppDbContext context)
            : base(context) { }

        public async Task<IEnumerable<UserRole>> GetByUserIdAsync(
            Guid userId,
            CancellationToken cancellationToken = default
        )
        {
            return await _context
                .Set<UserRole>()
                .Where(ur => ur.UserId == userId)
                .ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<UserRole>> GetByRoleIdAsync(
            Guid roleId,
            CancellationToken cancellationToken = default
        )
        {
            return await _context
                .Set<UserRole>()
                .Where(ur => ur.RoleId == roleId)
                .ToListAsync(cancellationToken);
        }

        public async Task<User?> GetUserWithRolesAsync(
            Guid userId,
            CancellationToken cancellationToken = default
        )
        {
            return await _context
                .Users.Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
                .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);
        }
    }
}

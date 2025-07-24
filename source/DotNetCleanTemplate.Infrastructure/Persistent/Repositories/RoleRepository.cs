using DotNetCleanTemplate.Domain.Entities;
using DotNetCleanTemplate.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace DotNetCleanTemplate.Infrastructure.Persistent.Repositories
{
    public class RoleRepository : BaseRepository, IRoleRepository
    {
        public RoleRepository(AppDbContext context)
            : base(context) { }

        public async Task<Role?> FindByNameAsync(
            string name,
            CancellationToken cancellationToken = default
        )
        {
            return await _context.Roles.FirstOrDefaultAsync(
                r => r.Name.Value == name,
                cancellationToken
            );
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

        public async Task<(List<Role> Roles, int TotalCount)> GetRolesPaginatedAsync(
            int page,
            int pageSize,
            CancellationToken cancellationToken = default
        )
        {
            var query = _context.Roles;

            var totalCount = await query.CountAsync(cancellationToken);
            var roles = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);

            return (roles, totalCount);
        }
    }
}

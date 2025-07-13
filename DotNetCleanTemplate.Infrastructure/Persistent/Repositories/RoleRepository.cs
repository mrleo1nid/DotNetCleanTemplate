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
    }
}

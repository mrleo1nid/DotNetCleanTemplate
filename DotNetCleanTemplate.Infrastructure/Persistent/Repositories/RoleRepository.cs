using Microsoft.EntityFrameworkCore;
using DotNetCleanTemplate.Domain.Entities;
using DotNetCleanTemplate.Domain.Repositories;

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
    }
}

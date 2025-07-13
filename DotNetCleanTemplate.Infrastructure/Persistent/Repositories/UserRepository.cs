using DotNetCleanTemplate.Domain.Entities;
using DotNetCleanTemplate.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace DotNetCleanTemplate.Infrastructure.Persistent.Repositories
{
    public class UserRepository : BaseRepository, IUserRepository
    {
        public UserRepository(AppDbContext context)
            : base(context) { }

        public async Task<User?> FindByEmailAsync(
            string email,
            CancellationToken cancellationToken = default
        )
        {
            return await _context.Users.FirstOrDefaultAsync(
                u => u.Email.Value == email,
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

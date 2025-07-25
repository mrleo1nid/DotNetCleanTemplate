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
            return await _context
                .Users.Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
                .FirstOrDefaultAsync(u => u.Email.Value == email, cancellationToken);
        }

        public async Task<User?> FindByUserNameAsync(
            string userName,
            CancellationToken cancellationToken = default
        )
        {
            return await _context
                .Users.Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
                .FirstOrDefaultAsync(u => u.Name.Value == userName, cancellationToken);
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

        public async Task<List<User>> GetAllUsersWithRolesAsync(
            CancellationToken cancellationToken = default
        )
        {
            return await _context
                .Users.Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
                .ToListAsync(cancellationToken);
        }

        public async Task<(List<User> Users, int TotalCount)> GetUsersWithRolesPaginatedAsync(
            int page,
            int pageSize,
            CancellationToken cancellationToken = default
        )
        {
            var query = _context.Users.Include(u => u.UserRoles).ThenInclude(ur => ur.Role);

            var totalCount = await query.CountAsync(cancellationToken);
            var users = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);

            return (users, totalCount);
        }

        public async Task<IEnumerable<User>> GetUsersByRoleAsync(
            Guid roleId,
            CancellationToken cancellationToken = default
        )
        {
            return await _context
                .Users.Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
                .Where(u => u.UserRoles.Any(ur => ur.RoleId == roleId))
                .ToListAsync(cancellationToken);
        }
    }
}

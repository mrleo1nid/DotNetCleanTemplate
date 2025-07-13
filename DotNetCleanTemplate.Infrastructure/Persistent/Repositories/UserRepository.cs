using Microsoft.EntityFrameworkCore;
using DotNetCleanTemplate.Domain.Entities;
using DotNetCleanTemplate.Domain.Repositories;

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
    }
}

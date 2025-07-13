using DotNetCleanTemplate.Domain.Entities;

namespace DotNetCleanTemplate.Domain.Repositories
{
    public interface IUserRepository : IRepository
    {
        Task<User?> FindByEmailAsync(string email, CancellationToken cancellationToken = default);
        Task<User?> GetUserWithRolesAsync(
            Guid userId,
            CancellationToken cancellationToken = default
        );
    }
}

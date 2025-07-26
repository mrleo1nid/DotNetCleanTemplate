using System.Linq.Expressions;
using DotNetCleanTemplate.Domain.Common;

namespace DotNetCleanTemplate.Domain.Repositories
{
    public interface IReadRepository<T>
        where T : Entity<Guid>
    {
        Task<T?> GetByIdAsync(Guid id);
        Task<bool> ExistsAsync(Expression<Func<T, bool>> predicate);
        Task<IEnumerable<T>> GetAllAsync();
        Task<IEnumerable<T>> GetAllAsync(Expression<Func<T, bool>> predicate);
        Task<int> CountAsync();
    }
}

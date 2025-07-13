using DotNetCleanTemplate.Domain.Common;
using System.Linq.Expressions;

namespace DotNetCleanTemplate.Domain.Repositories
{
    public interface IRepository
    {
        Task<T?> GetByIdAsync<T>(Guid id)
            where T : Entity<Guid>;
        Task<bool> ExistsAsync<T>(Expression<Func<T, bool>> predicate)
            where T : Entity<Guid>;
        Task<IEnumerable<T>> GetAllAsync<T>()
            where T : Entity<Guid>;
        Task<IEnumerable<T>> GetAllAsync<T>(Expression<Func<T, bool>> predicate)
            where T : Entity<Guid>;
        Task<int> CountAsync<T>()
            where T : Entity<Guid>;
        Task<T> AddAsync<T>(T entity)
            where T : Entity<Guid>;
        Task<T> UpdateAsync<T>(T entity)
            where T : Entity<Guid>;
        Task<T> DeleteAsync<T>(T entity)
            where T : Entity<Guid>;
        Task<int> SaveChangesAsync();
    }
}

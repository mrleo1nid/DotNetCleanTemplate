using DotNetCleanTemplate.Domain.Common;

namespace DotNetCleanTemplate.Domain.Repositories
{
    public interface IWriteRepository<T>
        where T : Entity<Guid>
    {
        Task<T> AddAsync(T entity);
        Task<T> UpdateAsync(T entity);
        Task<T> DeleteAsync(T entity);
    }
}

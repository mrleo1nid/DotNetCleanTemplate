using System.Linq.Expressions;
using DotNetCleanTemplate.Domain.Common;
using DotNetCleanTemplate.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace DotNetCleanTemplate.Infrastructure.Persistent.Repositories
{
    public abstract class BaseRepository<T> : IRepository<T>
        where T : Entity<Guid>
    {
        protected readonly AppDbContext _context;

        protected BaseRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<T?> GetByIdAsync(Guid id) => await _context.Set<T>().FindAsync(id);

        public async Task<bool> ExistsAsync(Expression<Func<T, bool>> predicate) =>
            await _context.Set<T>().AnyAsync(predicate);

        public async Task<IEnumerable<T>> GetAllAsync() => await _context.Set<T>().ToListAsync();

        public async Task<IEnumerable<T>> GetAllAsync(Expression<Func<T, bool>> predicate) =>
            await _context.Set<T>().Where(predicate).ToListAsync();

        public async Task<int> CountAsync() => await _context.Set<T>().CountAsync();

        public async Task<T> AddAsync(T entity)
        {
            await _context.Set<T>().AddAsync(entity);
            return entity;
        }

        public async Task<T> UpdateAsync(T entity)
        {
            _context.Set<T>().Update(entity);
            return await Task.FromResult(entity);
        }

        public async Task<T> DeleteAsync(T entity)
        {
            _context.Set<T>().Remove(entity);
            return await Task.FromResult(entity);
        }

        protected async Task<T> AddOrUpdateAsync(T entity, Expression<Func<T, bool>> predicate)
        {
            var existing = await _context.Set<T>().FirstOrDefaultAsync(predicate);

            if (existing != null)
            {
                // Обновляем существующую сущность
                _context.Entry(existing).CurrentValues.SetValues(entity);
                return existing;
            }
            else
            {
                // Добавляем новую сущность
                await _context.Set<T>().AddAsync(entity);
                return entity;
            }
        }
    }
}

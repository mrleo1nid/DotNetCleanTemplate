using System.Linq.Expressions;
using DotNetCleanTemplate.Domain.Common;
using DotNetCleanTemplate.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace DotNetCleanTemplate.Infrastructure.Persistent.Repositories
{
    public abstract class BaseRepository : IRepository
    {
        protected readonly AppDbContext _context;

        protected BaseRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<T?> GetByIdAsync<T>(Guid id)
            where T : Entity<Guid> => await _context.Set<T>().FindAsync(id);

        public async Task<bool> ExistsAsync<T>(Expression<Func<T, bool>> predicate)
            where T : Entity<Guid> => await _context.Set<T>().AnyAsync(predicate);

        public async Task<IEnumerable<T>> GetAllAsync<T>()
            where T : Entity<Guid> => await _context.Set<T>().ToListAsync();

        public async Task<IEnumerable<T>> GetAllAsync<T>(Expression<Func<T, bool>> predicate)
            where T : Entity<Guid> => await _context.Set<T>().Where(predicate).ToListAsync();

        public async Task<int> CountAsync<T>()
            where T : Entity<Guid> => await _context.Set<T>().CountAsync();

        public async Task<T> AddAsync<T>(T entity)
            where T : Entity<Guid>
        {
            await _context.Set<T>().AddAsync(entity);
            return entity;
        }

        public async Task<T> UpdateAsync<T>(T entity)
            where T : Entity<Guid>
        {
            _context.Set<T>().Update(entity);
            return await Task.FromResult(entity);
        }

        public async Task<T> DeleteAsync<T>(T entity)
            where T : Entity<Guid>
        {
            _context.Set<T>().Remove(entity);
            await _context.SaveChangesAsync();
            return entity;
        }

        public async Task<int> SaveChangesAsync() => await _context.SaveChangesAsync();

        protected async Task<T> AddOrUpdateAsync<T>(T entity, Expression<Func<T, bool>> predicate)
            where T : Entity<Guid>
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

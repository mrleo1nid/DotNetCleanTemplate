using Microsoft.EntityFrameworkCore;
using DotNetCleanTemplate.Domain.Common;
using DotNetCleanTemplate.Domain.Repositories;
using System.Linq.Expressions;

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
            await _context.SaveChangesAsync();
            return entity;
        }

        public async Task<T> UpdateAsync<T>(T entity)
            where T : Entity<Guid>
        {
            _context.Set<T>().Update(entity);
            await _context.SaveChangesAsync();
            return entity;
        }

        public async Task<T> DeleteAsync<T>(T entity)
            where T : Entity<Guid>
        {
            _context.Set<T>().Remove(entity);
            await _context.SaveChangesAsync();
            return entity;
        }

        public async Task<int> SaveChangesAsync() => await _context.SaveChangesAsync();
    }
}

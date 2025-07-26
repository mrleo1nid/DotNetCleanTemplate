using DotNetCleanTemplate.Domain.Common;

namespace DotNetCleanTemplate.Domain.Repositories
{
    public interface IRepository<T> : IReadRepository<T>, IWriteRepository<T>
        where T : Entity<Guid> { }
}

using Microsoft.EntityFrameworkCore;

namespace InfrastructureTests
{
    public abstract class RepositoryTestBase<TContext>
        where TContext : DbContext
    {
        protected static TContext CreateDbContext(
            Func<DbContextOptions<TContext>, TContext> factory
        )
        {
            var options = new DbContextOptionsBuilder<TContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;
            return factory(options);
        }
    }
}

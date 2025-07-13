using Microsoft.EntityFrameworkCore;
using DotNetCleanTemplate.Infrastructure.Persistent;

namespace ApplicationTests
{
    public abstract class TestBase
    {
        protected static AppDbContext CreateDbContext()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;
            return new AppDbContext(options);
        }
    }
}

using DotNetCleanTemplate.Infrastructure.Persistent;
using Microsoft.EntityFrameworkCore;

namespace DotNetCleanTemplate.UnitTests.Common
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

using System.Threading.Tasks;
using DotNetCleanTemplate.Infrastructure.Configurations;
using DotNetCleanTemplate.Infrastructure.Persistent;
using DotNetCleanTemplate.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Testcontainers.PostgreSql;
using Xunit;

namespace InfrastructureTests
{
    public class MigrationServiceTests : IAsyncLifetime
    {
        private readonly PostgreSqlContainer _pgContainer;

        public MigrationServiceTests()
        {
            _pgContainer = new PostgreSqlBuilder()
                .WithImage("postgres:16-alpine")
                .WithCleanUp(true)
                .Build();
        }

        public async Task InitializeAsync() => await _pgContainer.StartAsync();

        public async Task DisposeAsync() => await _pgContainer.DisposeAsync();

        [Fact]
        public async Task ApplyMigrationsIfEnabledAsync_AppliesMigrations_WhenEnabled()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseNpgsql(
                    _pgContainer.GetConnectionString(),
                    b => b.MigrationsAssembly("DotNetCleanTemplate.Infrastructure")
                )
                .Options;
            var context = new AppDbContext(options);
            var dbOptions = Options.Create(
                new DatabaseSettings { ApplyMigrationsOnStartup = true }
            );
            var logger = new LoggerFactory().CreateLogger<MigrationService>();
            var service = new MigrationService(context, dbOptions, logger);

            await service.ApplyMigrationsIfEnabledAsync();

            // Проверяем, что таблица Users создана
            var exists =
                await context
                    .Database.SqlQueryRaw<int>(
                        "SELECT COUNT(*) AS \"Value\" FROM information_schema.tables WHERE table_name = 'Users'"
                    )
                    .FirstAsync() > 0;
            Assert.True(exists);
        }
    }
}

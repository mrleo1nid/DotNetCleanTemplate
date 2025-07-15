using System.Threading.Tasks;
using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Configurations;
using DotNet.Testcontainers.Containers;
using DotNetCleanTemplate.Infrastructure.Configurations;
using DotNetCleanTemplate.Infrastructure.Persistent;
using DotNetCleanTemplate.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Npgsql;
using Testcontainers.PostgreSql;

namespace InfrastructureTests
{
    public class MigrationServiceTests
    {
        [Fact]
        public async Task ApplyMigrationsIfEnabledAsync_Disabled_LogsOnlyDisabled()
        {
            var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>().UseInMemoryDatabase(
                Guid.NewGuid().ToString()
            );
            using var dbContext = new AppDbContext(optionsBuilder.Options);
            var loggerMock = new Mock<ILogger<MigrationService>>();
            var options = Options.Create(new DatabaseSettings { ApplyMigrationsOnStartup = false });
            var service = new MigrationService(dbContext, options, loggerMock.Object);

            await service.ApplyMigrationsIfEnabledAsync();

            loggerMock.Verify(
                x =>
                    x.Log(
                        LogLevel.Information,
                        It.IsAny<EventId>(),
                        It.Is<It.IsAnyType>(
                            (v, t) =>
                                v.ToString()!
                                    .Contains("Automatic migration is disabled by configuration")
                        ),
                        null,
                        It.IsAny<Func<It.IsAnyType, Exception?, string>>()
                    ),
                Times.Once
            );
        }

        [Fact]
        public async Task ApplyMigrationsIfEnabledAsync_Enabled_CreatesTables_Postgres()
        {
            var postgres = new PostgreSqlBuilder()
                .WithDatabase("testdb")
                .WithUsername("postgres")
                .WithPassword("postgres")
                .Build();

            await postgres.StartAsync();

            var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>().UseNpgsql(
                postgres.GetConnectionString()
            );

            using var dbContext = new AppDbContext(optionsBuilder.Options);
            var loggerMock = new Mock<ILogger<MigrationService>>();
            var options = Options.Create(new DatabaseSettings { ApplyMigrationsOnStartup = true });
            var service = new MigrationService(dbContext, options, loggerMock.Object);

            await service.ApplyMigrationsIfEnabledAsync();

            // Проверяем, что таблица Roles реально создана
            var cmd = dbContext.Database.GetDbConnection().CreateCommand();
            cmd.CommandText = "SELECT to_regclass('public.\"Roles\"')::text";
            await dbContext.Database.OpenConnectionAsync();
            var result = await cmd.ExecuteScalarAsync();
            Assert.NotNull(result);
            Assert.NotEqual("null", result.ToString());

            await postgres.StopAsync();
        }
    }
}

using DotNetCleanTemplate.Infrastructure.Persistent;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Xunit;

namespace InfrastructureTests;

public class MigrationTests
{
    [Fact]
    public void AllMigrations_AppliedSuccessfully()
    {
        using var connection = new SqliteConnection("DataSource=:memory:");
        connection.Open();

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite(connection)
            .ConfigureWarnings(x => x.Ignore(RelationalEventId.PendingModelChangesWarning))
            .Options;

        using (var context = new AppDbContext(options))
        {
            // Smoke: просто проверяем, что миграции применяются без исключений
            context.Database.Migrate();
        }
    }
}

using System.Threading.Tasks;
using DotNetCleanTemplate.Domain.Services;
using DotNetCleanTemplate.Infrastructure.Configurations;
using DotNetCleanTemplate.Infrastructure.Persistent;
using DotNetCleanTemplate.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Testcontainers.PostgreSql;
using Xunit;

namespace InfrastructureTests
{
    public class InitDataServiceTests_Container : IAsyncLifetime
    {
        private readonly PostgreSqlContainer _pgContainer;
        private ServiceProvider _provider = null!;

        public InitDataServiceTests_Container()
        {
            _pgContainer = new PostgreSqlBuilder()
                .WithImage("postgres:16-alpine")
                .WithCleanUp(true)
                .Build();
        }

        public async Task InitializeAsync()
        {
            await _pgContainer.StartAsync();
            var services = new ServiceCollection();
            services.AddLogging();
            services.AddSingleton<IPasswordHasher, PasswordHasher>();
            services.Configure<InitDataConfig>(cfg =>
            {
                cfg.Roles = new() { new InitRoleConfig { Name = "Admin" } };
                cfg.Users = new()
                {
                    new InitUserConfig
                    {
                        UserName = "admin",
                        Email = "admin@example.com",
                        Password = "Admin123!",
                        Roles = new() { "Admin" },
                    },
                };
            });
            services.AddDbContext<AppDbContext>(options =>
                options.UseNpgsql(
                    _pgContainer.GetConnectionString(),
                    b => b.MigrationsAssembly("DotNetCleanTemplate.Infrastructure")
                )
            );
            services.AddScoped<InitDataService>();
            _provider = services.BuildServiceProvider();
            // Применяем миграции
            using var scope = _provider.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            await db.Database.MigrateAsync();
        }

        public async Task DisposeAsync() => await _pgContainer.DisposeAsync();

        [Fact]
        public async Task InitializeAsync_AddsRolesAndUsers()
        {
            using var scope = _provider.CreateScope();
            var service = scope.ServiceProvider.GetRequiredService<InitDataService>();
            await service.InitializeAsync();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            Assert.True(await db.Roles.AnyAsync(r => r.Name.Value == "Admin"));
            Assert.True(await db.Users.AnyAsync(u => u.Email.Value == "admin@example.com"));
        }
    }
}

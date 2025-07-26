using DotNetCleanTemplate.Domain.Entities;
using DotNetCleanTemplate.Domain.Factories.Entities;
using DotNetCleanTemplate.Domain.Services;
using DotNetCleanTemplate.Infrastructure.Configurations;
using DotNetCleanTemplate.Infrastructure.Persistent;
using DotNetCleanTemplate.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Testcontainers.PostgreSql;

namespace DotNetCleanTemplate.UnitTests.Infrastructure
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

    public class InitDataServiceUnitTests
    {
        private static AppDbContext CreateInMemoryDbContext()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;
            return new AppDbContext(options);
        }

        [Fact]
        public async Task InitializeAsync_EmptyConfig_LogsWarningAndDoesNothing()
        {
            using var dbContext = CreateInMemoryDbContext();
            var loggerMock = new Mock<ILogger<InitDataService>>();
            var options = Options.Create(new InitDataConfig { Roles = new(), Users = new() });
            var passwordHasher = new Mock<IPasswordHasher>().Object;
            var roleFactory = new Mock<IRoleFactory>().Object;
            var userFactory = new Mock<IUserFactory>().Object;
            var service = new InitDataService(
                dbContext,
                loggerMock.Object,
                options,
                passwordHasher,
                roleFactory,
                userFactory
            );

            await service.InitializeAsync();

            loggerMock.Verify(
                x =>
                    x.Log(
                        LogLevel.Warning,
                        It.IsAny<EventId>(),
                        It.Is<It.IsAnyType>(
                            (v, t) =>
                                v!.ToString()!.Contains("Init data config is empty or invalid.")
                        ),
                        null,
                        It.IsAny<Func<It.IsAnyType, Exception?, string>>()
                    ),
                Times.Once
            );
            Assert.Equal(0, await dbContext.Roles.CountAsync());
            Assert.Equal(0, await dbContext.Users.CountAsync());
        }

        [Fact]
        public async Task InitializeAsync_DuplicateUser_DoesNotAddAgain()
        {
            using var dbContext = CreateInMemoryDbContext();
            var roleNameFactory =
                new DotNetCleanTemplate.Infrastructure.Factories.Role.RoleNameFactory();
            var testRoleFactory =
                new DotNetCleanTemplate.Infrastructure.Factories.Entities.RoleFactory(
                    roleNameFactory
                );
            var emailFactory = new DotNetCleanTemplate.Infrastructure.Factories.User.EmailFactory();
            var userNameFactory =
                new DotNetCleanTemplate.Infrastructure.Factories.User.UserNameFactory();
            var passwordHashFactory =
                new DotNetCleanTemplate.Infrastructure.Factories.User.PasswordHashFactory();
            var testUserFactory =
                new DotNetCleanTemplate.Infrastructure.Factories.Entities.UserFactory(
                    emailFactory,
                    userNameFactory,
                    passwordHashFactory
                );

            dbContext.Roles.Add(testRoleFactory.Create("Admin"));
            dbContext.Users.Add(
                testUserFactory.Create("admin", "admin@example.com", "sdfsdfdsfsdfggdffgdg1")
            );
            await dbContext.SaveChangesAsync();

            var loggerMock = new Mock<ILogger<InitDataService>>();
            var options = Options.Create(
                new InitDataConfig
                {
                    Roles = new List<InitRoleConfig> { new() { Name = "Admin" } },
                    Users = new List<InitUserConfig>
                    {
                        new()
                        {
                            UserName = "admin",
                            Email = "admin@example.com",
                            Password = "pass",
                            Roles = new() { "Admin" },
                        },
                    },
                }
            );
            var passwordHasher = new Mock<IPasswordHasher>();
            passwordHasher.Setup(x => x.HashPassword(It.IsAny<string>())).Returns("hash");

            var roleFactory = new Mock<IRoleFactory>().Object;
            var userFactory = new Mock<IUserFactory>().Object;
            var service = new InitDataService(
                dbContext,
                loggerMock.Object,
                options,
                passwordHasher.Object,
                roleFactory,
                userFactory
            );
            await service.InitializeAsync();
            Assert.Equal(1, await dbContext.Users.CountAsync()); // не добавился второй раз
        }

        private class ThrowingSaveChangesDbContext : AppDbContext
        {
            public ThrowingSaveChangesDbContext(DbContextOptions<AppDbContext> options)
                : base(options) { }

            public override Task<int> SaveChangesAsync(
                CancellationToken cancellationToken = default
            ) => throw new DbUpdateException("fail");
        }

        [Fact]
        public async Task InitializeAsync_SaveChangesThrows_ThrowsException()
        {
            var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>().UseInMemoryDatabase(
                Guid.NewGuid().ToString()
            );
            using var dbContext = new ThrowingSaveChangesDbContext(optionsBuilder.Options);
            var loggerMock = new Mock<ILogger<InitDataService>>();
            var options = Options.Create(
                new InitDataConfig
                {
                    Roles = new List<InitRoleConfig> { new() { Name = "Admin" } },
                    Users = new List<InitUserConfig>
                    {
                        new()
                        {
                            UserName = "admin",
                            Email = "admin@example.com",
                            Password = "pass",
                            Roles = new() { "Admin" },
                        },
                    },
                }
            );
            var passwordHasher = new Mock<IPasswordHasher>();
            passwordHasher.Setup(x => x.HashPassword(It.IsAny<string>())).Returns("hash");

            var roleFactory = new Mock<IRoleFactory>().Object;
            var userFactory = new Mock<IUserFactory>().Object;
            var service = new InitDataService(
                dbContext,
                loggerMock.Object,
                options,
                passwordHasher.Object,
                roleFactory,
                userFactory
            );
            await Assert.ThrowsAsync<DbUpdateException>(() => service.InitializeAsync());
        }
    }

    // Вспомогательный класс для моков DbSet
    public static class DbSetMock
    {
        public static DbSet<T> Create<T>(IEnumerable<T> elements)
            where T : class
        {
            var queryable = elements.AsQueryable();
            var dbSet = new Mock<DbSet<T>>();
            dbSet.As<IQueryable<T>>().Setup(m => m.Provider).Returns(queryable.Provider);
            dbSet.As<IQueryable<T>>().Setup(m => m.Expression).Returns(queryable.Expression);
            dbSet.As<IQueryable<T>>().Setup(m => m.ElementType).Returns(queryable.ElementType);
            dbSet
                .As<IQueryable<T>>()
                .Setup(m => m.GetEnumerator())
                .Returns(queryable.GetEnumerator());
            return dbSet.Object;
        }
    }
}

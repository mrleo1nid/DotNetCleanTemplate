using DotNetCleanTemplate.Application.DependencyExtensions;
using DotNetCleanTemplate.Application.Interfaces;
using DotNetCleanTemplate.Domain.Factories.User;
using DotNetCleanTemplate.Domain.Repositories;
using DotNetCleanTemplate.Domain.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace DotNetCleanTemplate.UnitTests.Api
{
    public class ApplicationServiceExtensionsTests
    {
        [Fact]
        public void AddApplicationServices_RegistersServicesCorrectly()
        {
            var services = new ServiceCollection();
            // Регистрируем моки для зависимостей сервисов
            services.AddScoped(_ => new Mock<IUserRepository>().Object);
            services.AddScoped(_ => new Mock<IRoleRepository>().Object);
            services.AddScoped(_ => new Mock<IUnitOfWork>().Object);
            services.AddScoped(_ => new Mock<IPasswordHasher>().Object);
            services.AddScoped(_ => new Mock<IPasswordHashFactory>().Object);
            var config = new ConfigurationBuilder()
                .AddInMemoryCollection(
                    new Dictionary<string, string?>
                    {
                        { "Performance:LongRunningThresholdMs", "500" },
                    }
                )
                .Build();
            services.AddApplicationServices(config);
            var provider = services.BuildServiceProvider();

            var userService = provider.GetService<IUserService>();
            var roleService = provider.GetService<IRoleService>();

            Assert.NotNull(userService);
            Assert.NotNull(roleService);
        }
    }
}

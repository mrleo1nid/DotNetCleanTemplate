using DotNetCleanTemplate.Application.DependencyExtensions;
using DotNetCleanTemplate.Application.Interfaces;
using DotNetCleanTemplate.Application.Services;
using DotNetCleanTemplate.Domain.Repositories;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;

namespace ApplicationTests
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
            services.AddApplicationServices();
            var provider = services.BuildServiceProvider();

            var userService = provider.GetService<IUserService>();
            var roleService = provider.GetService<IRoleService>();

            Assert.NotNull(userService);
            Assert.NotNull(roleService);
        }
    }
}

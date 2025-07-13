using DotNetCleanTemplate.Domain.Repositories;
using DotNetCleanTemplate.Infrastructure.DependencyExtensions;
using DotNetCleanTemplate.Infrastructure.Persistent;
using DotNetCleanTemplate.Infrastructure.Persistent.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace InfrastructureTests
{
    public class InfrastructureServiceExtensionsTests
    {
        [Fact]
        public void AddInfrastructure_RegistersRepositoriesCorrectly()
        {
            // Arrange
            var services = new ServiceCollection();
            services.AddDbContext<AppDbContext>(options => options.UseInMemoryDatabase("TestDb"));

            // Act
            services.AddInfrastructure();
            var provider = services.BuildServiceProvider();

            // Assert
            var userRepo = provider.GetService<IUserRepository>();
            var roleRepo = provider.GetService<IRoleRepository>();
            var dbContext = provider.GetService<AppDbContext>();

            Assert.NotNull(userRepo);
            Assert.NotNull(roleRepo);
            Assert.NotNull(dbContext);
            Assert.IsType<UserRepository>(userRepo);
            Assert.IsType<RoleRepository>(roleRepo);
            Assert.IsType<AppDbContext>(dbContext);
        }
    }
}

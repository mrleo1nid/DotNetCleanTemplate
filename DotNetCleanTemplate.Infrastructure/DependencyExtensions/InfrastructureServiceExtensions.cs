using Microsoft.Extensions.DependencyInjection;
using DotNetCleanTemplate.Domain.Repositories;
using DotNetCleanTemplate.Infrastructure.Persistent.Repositories;

namespace DotNetCleanTemplate.Infrastructure.DependencyExtensions
{
    public static class InfrastructureServiceExtensions
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services)
        {
            // Register repositories
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<IRoleRepository, RoleRepository>();
            services.AddScoped<IUnitOfWork, UnitOfWork>();

            return services;
        }
    }
}

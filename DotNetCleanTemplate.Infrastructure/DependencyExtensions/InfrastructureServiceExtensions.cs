using CacheManager.Core;
using CacheManager.Core.Configuration;
using DotNetCleanTemplate.Domain.Repositories;
using DotNetCleanTemplate.Infrastructure.Configurations;
using DotNetCleanTemplate.Infrastructure.Persistent.Repositories;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace DotNetCleanTemplate.Infrastructure.DependencyExtensions
{
    public static class InfrastructureServiceExtensions
    {
        public static IServiceCollection AddInfrastructure(
            this IServiceCollection services,
            IConfiguration configuration
        )
        {
            // Register JwtSettings
            services.Configure<JwtSettings>(configuration.GetSection("JwtSettings"));

            // Register cache
            var cacheConfiguration = configuration.GetCacheConfiguration();
            services.AddSingleton(CacheFactory.FromConfiguration<string>(cacheConfiguration));

            // Register repositories
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<IRoleRepository, RoleRepository>();
            services.AddScoped<IUserRoleRepository, UserRoleRepository>();
            services.AddScoped<IUnitOfWork, UnitOfWork>();
            return services;
        }
    }
}

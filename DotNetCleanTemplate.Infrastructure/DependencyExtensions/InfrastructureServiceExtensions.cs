using CacheManager.Core;
using DotNetCleanTemplate.Domain.Repositories;
using DotNetCleanTemplate.Domain.Services;
using DotNetCleanTemplate.Infrastructure.Configurations;
using DotNetCleanTemplate.Infrastructure.Persistent.Repositories;
using DotNetCleanTemplate.Infrastructure.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

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
            services.Configure<DatabaseSettings>(configuration.GetSection("Database"));
            services.Configure<InitDataConfig>(configuration.GetSection("InitData"));

            // Register cache
            var cacheConfiguration = configuration.GetCacheConfiguration();
            services.AddSingleton(CacheFactory.FromConfiguration<object>(cacheConfiguration));
            services.AddSingleton<ICacheService, CacheService>();

            // Register repositories
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<IRoleRepository, RoleRepository>();
            services.AddScoped<IUserRoleRepository, UserRoleRepository>();
            services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
            services.AddScoped<IUnitOfWork, UnitOfWork>();
            services.AddScoped<ITokenService, TokenService>();
            services.AddScoped<IPasswordHasher, PasswordHasher>();
            services.AddScoped<MigrationService>();
            services.AddScoped<InitDataService>();

            return services;
        }
    }
}

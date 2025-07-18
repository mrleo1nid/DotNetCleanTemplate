using CacheManager.Core;
using DotNetCleanTemplate.Domain.Repositories;
using DotNetCleanTemplate.Domain.Services;
using DotNetCleanTemplate.Infrastructure.Configurations;
using DotNetCleanTemplate.Infrastructure.Persistent.Repositories;
using DotNetCleanTemplate.Infrastructure.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace DotNetCleanTemplate.Infrastructure.DependencyExtensions
{
    public static class InfrastructureServiceExtensions
    {
        public static IServiceCollection AddInfrastructure(
            this IServiceCollection services,
            IConfiguration configuration
        )
        {
            // Register settings
            services
                .Configure<JwtSettings>(configuration.GetSection(JwtSettings.SectionName))
                .Configure<DatabaseSettings>(configuration.GetSection(DatabaseSettings.SectionName))
                .Configure<InitDataConfig>(configuration.GetSection(InitDataConfig.SectionName))
                .Configure<TokenCleanupSettings>(
                    configuration.GetSection(TokenCleanupSettings.SectionName)
                )
                .Configure<UserLockoutCleanupSettings>(
                    configuration.GetSection(UserLockoutCleanupSettings.SectionName)
                );

            // Register cache
            var cacheConfiguration = configuration.GetCacheConfiguration();
            services.AddSingleton(CacheFactory.FromConfiguration<object>(cacheConfiguration));
            services.AddSingleton<ICacheService, CacheService>();

            // HealthCheck
            services.AddScoped<IHealthCheckService, HealthCheckService>();

            // Register repositories
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<IRoleRepository, RoleRepository>();
            services.AddScoped<IUserRoleRepository, UserRoleRepository>();
            services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
            services.AddScoped<IUserLockoutRepository, UserLockoutRepository>();
            services.AddScoped<IUnitOfWork, UnitOfWork>();
            services.AddScoped<ITokenService, TokenService>();
            services.AddScoped<IPasswordHasher, PasswordHasher>();
            services.AddScoped<MigrationService>();
            services.AddScoped<InitDataService>();

            // Register background services
            services.AddHostedService<ExpiredTokenCleanupService>();
            services.AddHostedService<UserLockoutCleanupService>();

            return services;
        }
    }
}

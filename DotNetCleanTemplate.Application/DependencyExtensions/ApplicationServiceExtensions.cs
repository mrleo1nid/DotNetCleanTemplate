using DotNetCleanTemplate.Application.Behaviors;
using DotNetCleanTemplate.Application.Configurations;
using DotNetCleanTemplate.Application.Interfaces;
using DotNetCleanTemplate.Application.Services;
using DotNetCleanTemplate.Domain.Decorators;
using FluentValidation;
using Mapster;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace DotNetCleanTemplate.Application.DependencyExtensions
{
    public static class ApplicationServiceExtensions
    {
        public static IServiceCollection AddApplicationServices(
            this IServiceCollection services,
            IConfiguration configuration
        )
        {
            services.Configure<PerformanceSettings>(
                configuration.GetSection(PerformanceSettings.SectionName)
            );

            services.Configure<FailToBanSettings>(
                configuration.GetSection(FailToBanSettings.SectionName)
            );

            services.AddScoped<IUserService, UserService>();
            services.AddScoped<IRoleService, RoleService>();
            services.AddScoped<IUserLockoutService, UserLockoutService>();

            // Application layer services that coordinate with Infrastructure
            services.AddScoped<IMigrationServiceDecorator, ApplicationMigrationService>();
            services.AddScoped<IInitDataServiceDecorator, ApplicationInitDataService>();

            TypeAdapterConfig.GlobalSettings.Scan(Assembly.GetExecutingAssembly());

            services.AddMapster();
            services.AddMediatR(cfg =>
                cfg.RegisterServicesFromAssembly(typeof(ApplicationServiceExtensions).Assembly)
            );

            services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());

            services.AddTransient(typeof(IPipelineBehavior<,>), typeof(CachingBehavior<,>));
            services.AddTransient(typeof(IPipelineBehavior<,>), typeof(PerformanceBehaviour<,>));
            services.AddTransient(typeof(IPipelineBehavior<,>), typeof(MetricsBehavior<,>));

            return services;
        }
    }
}

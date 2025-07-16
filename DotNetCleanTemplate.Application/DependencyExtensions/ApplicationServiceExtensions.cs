using System.Reflection;
using DotNetCleanTemplate.Application.Behaviors;
using DotNetCleanTemplate.Application.Interfaces;
using DotNetCleanTemplate.Application.Services;
using Mapster;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace DotNetCleanTemplate.Application.DependencyExtensions
{
    public static class ApplicationServiceExtensions
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services)
        {
            services.AddScoped<IUserService, UserService>();
            services.AddScoped<IRoleService, RoleService>();

            TypeAdapterConfig.GlobalSettings.Scan(Assembly.GetExecutingAssembly());

            services.AddMapster();
            services.AddMediatR(cfg =>
                cfg.RegisterServicesFromAssembly(typeof(ApplicationServiceExtensions).Assembly)
            );

            services.AddTransient(typeof(IPipelineBehavior<,>), typeof(CachingBehavior<,>));

            return services;
        }
    }
}

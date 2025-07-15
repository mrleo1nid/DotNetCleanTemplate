using DotNetCleanTemplate.Application.Interfaces;
using DotNetCleanTemplate.Application.Services;
using Mapster;
using Microsoft.Extensions.DependencyInjection;

namespace DotNetCleanTemplate.Application.DependencyExtensions
{
    public static class ApplicationServiceExtensions
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services)
        {
            services.AddScoped<IUserService, UserService>();
            services.AddScoped<IRoleService, RoleService>();
            services.AddMapster();
            services.AddMediatR(cfg =>
                cfg.RegisterServicesFromAssembly(typeof(ApplicationServiceExtensions).Assembly)
            );

            return services;
        }
    }
}

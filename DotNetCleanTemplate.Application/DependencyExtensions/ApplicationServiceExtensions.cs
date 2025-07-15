using DotNetCleanTemplate.Application.Interfaces;
using DotNetCleanTemplate.Application.Services;
using Microsoft.Extensions.DependencyInjection;

namespace DotNetCleanTemplate.Application.DependencyExtensions
{
    public static class ApplicationServiceExtensions
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services)
        {
            services.AddScoped<IUserService, UserService>();
            services.AddScoped<IRoleService, RoleService>();
            // Если потребуется использовать ITokenService напрямую в application слое:
            // services.AddScoped<ITokenService, TokenService>();
            services.AddAutoMapper(typeof(ApplicationServiceExtensions).Assembly);
            services.AddMediatR(cfg =>
                cfg.RegisterServicesFromAssembly(typeof(ApplicationServiceExtensions).Assembly)
            );

            return services;
        }
    }
}

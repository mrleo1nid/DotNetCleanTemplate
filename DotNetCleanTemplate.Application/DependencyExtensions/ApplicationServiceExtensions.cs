using AutoMapper;
using DotNetCleanTemplate.Application.Services;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace DotNetCleanTemplate.Application.DependencyExtensions
{
    public static class ApplicationServiceExtensions
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services)
        {
            services.AddScoped<UserService>();
            services.AddScoped<RoleService>();
            services.AddAutoMapper(typeof(ApplicationServiceExtensions).Assembly);
            services.AddMediatR(cfg =>
                cfg.RegisterServicesFromAssembly(typeof(ApplicationServiceExtensions).Assembly)
            );

            return services;
        }
    }
}

namespace DotNetCleanTemplate.Api.DependencyExtensions
{
    public static class CorsExtension
    {
        public static void AddCors(this IServiceCollection services, IConfiguration configuration)
        {
            var allowedOrigins = configuration.GetSection("Cors:AllowedOrigins").Get<string[]>();

            if (allowedOrigins != null)
            {
                services.AddCors(options =>
                {
                    options.AddPolicy(
                        "AllowConfiguredOrigins",
                        policy =>
                        {
                            policy
                                .WithOrigins(
                                    allowedOrigins
                                        .Where(origin => !string.IsNullOrEmpty(origin))
                                        .ToArray()
                                )
                                .AllowAnyHeader()
                                .AllowAnyMethod();
                        }
                    );
                });
            }
        }

        public static IApplicationBuilder UseCorsExtension(this IApplicationBuilder app)
        {
            app.UseCors("AllowConfiguredOrigins");
            return app;
        }
    }
}

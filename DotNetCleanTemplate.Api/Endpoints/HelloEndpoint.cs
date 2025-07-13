using FastEndpoints;

namespace DotNetCleanTemplate.Api.Endpoints;

public class HelloEndpoint : EndpointWithoutRequest<HelloResponse>
{
    public override void Configure()
    {
        Get("/hello");
        AllowAnonymous();
        Description(b =>
            b.WithSummary("Пример FastEndpoints endpoint")
                .WithDescription(
                    "Минимальный GET endpoint для демонстрации работы FastEndpoints и Swagger"
                )
        );
    }

    public override Task HandleAsync(CancellationToken ct)
    {
        return SendAsync(
            new HelloResponse { Message = "Hello from FastEndpoints!" },
            cancellation: ct
        );
    }
}

public class HelloResponse
{
    public string Message { get; set; } = string.Empty;
}

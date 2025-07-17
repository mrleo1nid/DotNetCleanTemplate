using DotNetCleanTemplate.Shared.Common;
using FastEndpoints;

namespace DotNetCleanTemplate.Api.Endpoints;

public class HelloEndpoint : EndpointWithoutRequest<Result<HelloResponse>>
{
    public override void Configure()
    {
        Get("/tests/hello");
        AllowAnonymous();
        Tags("Test");
        Throttle(hitLimit: 120, durationSeconds: 60, headerName: "X-Client-Id");
        Description(b =>
            b.WithSummary("Пример FastEndpoints endpoint")
                .WithDescription(
                    "Минимальный GET endpoint для демонстрации работы FastEndpoints и Swagger"
                )
        );
    }

    public override Task HandleAsync(CancellationToken ct)
    {
        var response = new HelloResponse { Message = "Hello from FastEndpoints!" };
        return SendAsync(Result<HelloResponse>.Success(response), cancellation: ct);
    }
}

public class HelloResponse
{
    public string Message { get; set; } = string.Empty;
}

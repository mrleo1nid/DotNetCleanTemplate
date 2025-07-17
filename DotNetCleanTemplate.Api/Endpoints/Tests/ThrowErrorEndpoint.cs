using FastEndpoints;

namespace DotNetCleanTemplate.Api.Endpoints;

public class ThrowErrorEndpoint : EndpointWithoutRequest
{
    public override void Configure()
    {
        Get("/tests/throw-error");
        AllowAnonymous();
        Tags("Test");
        Throttle(hitLimit: 120, durationSeconds: 60, headerName: "X-Client-Id");
    }

    public override Task HandleAsync(CancellationToken ct)
    {
        throw new InvalidOperationException("Test exception for middleware");
    }
}

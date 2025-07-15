using FastEndpoints;

namespace DotNetCleanTemplate.Api.Endpoints;

public class ThrowErrorEndpoint : EndpointWithoutRequest
{
    public override void Configure()
    {
        Get("/throw-error");
        AllowAnonymous();
        Tags("Test");
    }

    public override Task HandleAsync(CancellationToken ct)
    {
        throw new Exception("Test exception for middleware");
    }
}

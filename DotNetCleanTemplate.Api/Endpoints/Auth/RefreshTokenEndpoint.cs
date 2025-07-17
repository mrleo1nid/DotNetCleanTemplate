using DotNetCleanTemplate.Application.Features.Auth.Refresh;
using DotNetCleanTemplate.Shared.Common;
using DotNetCleanTemplate.Shared.DTOs;
using FastEndpoints;
using MediatR;

namespace DotNetCleanTemplate.Api.Endpoints;

public class RefreshTokenEndpoint
    : Endpoint<RefreshTokenRequestDto, Result<RefreshTokenResponseDto>>
{
    private readonly IMediator _mediator;

    public RefreshTokenEndpoint(IMediator mediator)
    {
        _mediator = mediator;
    }

    public override void Configure()
    {
        Post("/auth/refresh");
        AllowAnonymous();
        Tags("Auth");
        Throttle(hitLimit: 120, durationSeconds: 60, headerName: "X-Client-Id");
        Description(b =>
            b.WithSummary("Обновление refresh/access токена")
                .WithDescription(
                    "Получить новый refresh и access токен по действующему refresh токену"
                )
        );
    }

    public override async Task HandleAsync(RefreshTokenRequestDto req, CancellationToken ct)
    {
        var command = new RefreshTokenCommand(req.RefreshToken);
        var result = await _mediator.Send(command, ct);
        await SendAsync(result, cancellation: ct);
    }
}

public class RefreshTokenRequestDto
{
    public string RefreshToken { get; set; } = default!;
}

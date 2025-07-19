using DotNetCleanTemplate.Application.Features.Auth.Login;
using DotNetCleanTemplate.Infrastructure.Configurations;
using DotNetCleanTemplate.Shared.Common;
using DotNetCleanTemplate.Shared.DTOs;
using FastEndpoints;
using MediatR;

namespace DotNetCleanTemplate.Api.Endpoints;

public class LoginEndpoint : Endpoint<LoginRequestDto, Result<LoginResponseDto>>
{
    private readonly IMediator _mediator;

    public LoginEndpoint(IMediator mediator)
    {
        _mediator = mediator;
    }

    public override void Configure()
    {
        Post("/auth/login");
        AllowAnonymous();
        Tags("Auth");
        Description(b =>
            b.WithSummary("Аутентификация пользователя")
                .WithDescription("Получить access и refresh токены по email и паролю")
        );
    }

    public override async Task HandleAsync(LoginRequestDto req, CancellationToken ct)
    {
        var command = new LoginCommand(req);
        var result = await _mediator.Send(command, ct);

        if (result.IsSuccess)
        {
            await SendAsync(result, cancellation: ct);
        }
        else
        {
            await SendAsync(result, StatusCodes.Status401Unauthorized, cancellation: ct);
        }
    }
}

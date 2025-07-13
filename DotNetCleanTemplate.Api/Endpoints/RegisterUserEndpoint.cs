using FastEndpoints;
using MediatR;
using DotNetCleanTemplate.Application.Features.Auth.RegisterUser;
using DotNetCleanTemplate.Shared.DTOs;

namespace DotNetCleanTemplate.Api.Endpoints;

public class RegisterUserEndpoint : Endpoint<RegisterUserDto, Guid>
{
    private readonly IMediator _mediator;

    public RegisterUserEndpoint(IMediator mediator)
    {
        _mediator = mediator;
    }

    public override void Configure()
    {
        Post("/auth/register");
        AllowAnonymous();
        Tags("Auth");
        Description(b =>
            b.WithSummary("Регистрация пользователя")
                .WithDescription("Создать нового пользователя и вернуть его идентификатор")
        );
    }

    public override async Task HandleAsync(RegisterUserDto req, CancellationToken ct)
    {
        var command = new RegisterUserCommand { Dto = req };
        var userId = await _mediator.Send(command, ct);
        await SendAsync(userId, cancellation: ct);
    }
}

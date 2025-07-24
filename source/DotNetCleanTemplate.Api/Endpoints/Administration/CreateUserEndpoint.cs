using DotNetCleanTemplate.Application.Configurations;
using DotNetCleanTemplate.Application.Features.Users.CreateUser;
using DotNetCleanTemplate.Application.Validation;
using DotNetCleanTemplate.Shared.Common;
using DotNetCleanTemplate.Shared.DTOs;
using FastEndpoints;
using MediatR;
using Microsoft.Extensions.Options;

namespace DotNetCleanTemplate.Api.Endpoints;

public class CreateUserEndpoint : Endpoint<CreateUserDto, Result<Guid>>
{
    private readonly IMediator _mediator;
    private readonly DefaultSettings _defaultSettings;

    public CreateUserEndpoint(IMediator mediator, IOptions<DefaultSettings> defaultSettings)
    {
        _mediator = mediator;
        _defaultSettings = defaultSettings.Value;
    }

    public override void Configure()
    {
        Post("/administration/users");
        Tags("Administration");
        Roles(_defaultSettings.DefaultAdminRole ?? "Admin");
        Validator<CreateUserDtoValidator>();
        Description(b =>
            b.WithSummary("Создать пользователя")
                .WithDescription("Создать нового пользователя (только для администраторов)")
        );
    }

    public override async Task HandleAsync(CreateUserDto req, CancellationToken ct)
    {
        var command = new CreateUserCommand { Dto = req };
        var result = await _mediator.Send(command, ct);
        await SendAsync(result, cancellation: ct);
    }
}

using DotNetCleanTemplate.Application.Configurations;
using DotNetCleanTemplate.Application.Features.Users;
using DotNetCleanTemplate.Shared.Common;
using DotNetCleanTemplate.Shared.DTOs;
using FastEndpoints;
using MediatR;
using Microsoft.Extensions.Options;

namespace DotNetCleanTemplate.Api.Endpoints
{
    public class ChangeUserPasswordEndpoint : Endpoint<ChangeUserPasswordDto, Result<Unit>>
    {
        private readonly IMediator _mediator;
        private readonly DefaultSettings _defaultSettings;

        public ChangeUserPasswordEndpoint(
            IMediator mediator,
            IOptions<DefaultSettings> defaultSettings
        )
        {
            _mediator = mediator;
            _defaultSettings = defaultSettings.Value;
        }

        public override void Configure()
        {
            Put("/administration/users/{userId}/password");
            Tags("Users");
            Roles(_defaultSettings.DefaultAdminRole ?? "Admin");
            Description(b =>
                b.WithSummary("Изменить пароль пользователя")
                    .WithDescription("Изменяет пароль пользователя в системе")
            );
        }

        public override async Task HandleAsync(ChangeUserPasswordDto req, CancellationToken ct)
        {
            var userId = Route<Guid>("userId");
            req.UserId = userId; // Устанавливаем UserId из маршрута

            var command = new ChangeUserPasswordCommand { Dto = req };
            var result = await _mediator.Send(command, ct);
            await SendAsync(result, cancellation: ct);
        }
    }
}

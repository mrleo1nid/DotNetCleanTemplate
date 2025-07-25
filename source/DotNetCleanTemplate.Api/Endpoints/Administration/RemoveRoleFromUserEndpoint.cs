using DotNetCleanTemplate.Application.Configurations;
using DotNetCleanTemplate.Application.Features.Users;
using DotNetCleanTemplate.Shared.Common;
using DotNetCleanTemplate.Shared.DTOs;
using FastEndpoints;
using MediatR;
using Microsoft.Extensions.Options;

namespace DotNetCleanTemplate.Api.Endpoints
{
    public class RemoveRoleFromUserEndpoint : Endpoint<RemoveRoleFromUserDto, Result<Unit>>
    {
        private readonly IMediator _mediator;
        private readonly DefaultSettings _defaultSettings;

        public RemoveRoleFromUserEndpoint(
            IMediator mediator,
            IOptions<DefaultSettings> defaultSettings
        )
        {
            _mediator = mediator;
            _defaultSettings = defaultSettings.Value;
        }

        public override void Configure()
        {
            Post("/administration/remove-role");
            Tags("Roles");
            Roles(_defaultSettings.DefaultAdminRole ?? "Admin");
            Description(b =>
                b.WithSummary("Удалить роль у пользователя")
                    .WithDescription(
                        "Удаляет роль у пользователя по идентификаторам пользователя и роли"
                    )
            );
        }

        public override async Task HandleAsync(RemoveRoleFromUserDto req, CancellationToken ct)
        {
            var command = new RemoveRoleFromUserCommand { Dto = req };
            var result = await _mediator.Send(command, ct);
            await SendAsync(result, cancellation: ct);
        }
    }
}

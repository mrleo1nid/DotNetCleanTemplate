using DotNetCleanTemplate.Application.Configurations;
using DotNetCleanTemplate.Application.Features.Users;
using DotNetCleanTemplate.Shared.Common;
using DotNetCleanTemplate.Shared.DTOs;
using FastEndpoints;
using MediatR;
using Microsoft.Extensions.Options;

namespace DotNetCleanTemplate.Api.Endpoints
{
    public class CreateRoleEndpoint : Endpoint<CreateRoleDto, Result<RoleDto>>
    {
        private readonly IMediator _mediator;
        private readonly DefaultSettings _defaultSettings;

        public CreateRoleEndpoint(IMediator mediator, IOptions<DefaultSettings> defaultSettings)
        {
            _mediator = mediator;
            _defaultSettings = defaultSettings.Value;
        }

        public override void Configure()
        {
            Post("/administration/roles");
            Tags("Roles");
            Roles(_defaultSettings.DefaultAdminRole ?? "Admin");
            Description(b =>
                b.WithSummary("Создать новую роль").WithDescription("Создает новую роль в системе")
            );
        }

        public override async Task HandleAsync(CreateRoleDto req, CancellationToken ct)
        {
            var command = new CreateRoleCommand { Name = req.Name };
            var result = await _mediator.Send(command, ct);
            await SendAsync(result, cancellation: ct);
        }
    }
}

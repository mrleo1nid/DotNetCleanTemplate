using DotNetCleanTemplate.Application.Configurations;
using DotNetCleanTemplate.Application.Features.Users;
using DotNetCleanTemplate.Shared.Common;
using FastEndpoints;
using MediatR;
using Microsoft.Extensions.Options;

namespace DotNetCleanTemplate.Api.Endpoints
{
    public class DeleteRoleEndpoint : EndpointWithoutRequest<Result<Unit>>
    {
        private readonly IMediator _mediator;
        private readonly DefaultSettings _defaultSettings;

        public DeleteRoleEndpoint(IMediator mediator, IOptions<DefaultSettings> defaultSettings)
        {
            _mediator = mediator;
            _defaultSettings = defaultSettings.Value;
        }

        public override void Configure()
        {
            Delete("/administration/roles/{roleId}");
            Tags("Roles");
            Roles(_defaultSettings.DefaultAdminRole ?? "Admin");
            Description(b =>
                b.WithSummary("Удалить роль").WithDescription("Удаляет роль из системы")
            );
        }

        public override async Task HandleAsync(CancellationToken ct)
        {
            var roleId = Route<Guid>("roleId");
            var command = new DeleteRoleCommand { RoleId = roleId };
            var result = await _mediator.Send(command, ct);
            await SendAsync(result, cancellation: ct);
        }
    }
}

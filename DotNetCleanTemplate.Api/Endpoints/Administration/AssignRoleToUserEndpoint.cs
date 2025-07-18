using DotNetCleanTemplate.Application.Features.Users;
using DotNetCleanTemplate.Infrastructure.Configurations;
using DotNetCleanTemplate.Shared.Common;
using DotNetCleanTemplate.Shared.DTOs;
using FastEndpoints;
using MediatR;

namespace DotNetCleanTemplate.Api.Endpoints
{
    public class AssignRoleToUserEndpoint : Endpoint<AssignRoleToUserDto, Result<Unit>>
    {
        private readonly IMediator _mediator;

        public AssignRoleToUserEndpoint(IMediator mediator)
        {
            _mediator = mediator;
        }

        public override void Configure()
        {
            Post("/administration/assign-role");
            Tags("Roles");
            Roles("Admin");
            Description(b =>
                b.WithSummary("Назначить роль пользователю")
                    .WithDescription(
                        "Назначает роль пользователю по идентификаторам пользователя и роли"
                    )
            );
        }

        public override async Task HandleAsync(AssignRoleToUserDto req, CancellationToken ct)
        {
            var command = new AssignRoleToUserCommand { Dto = req };
            var result = await _mediator.Send(command, ct);
            await SendAsync(result, cancellation: ct);
        }
    }
}

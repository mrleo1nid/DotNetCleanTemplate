using DotNetCleanTemplate.Application.Features.Users;
using DotNetCleanTemplate.Infrastructure.Configurations;
using DotNetCleanTemplate.Shared.Common;
using DotNetCleanTemplate.Shared.DTOs;
using FastEndpoints;
using MediatR;

namespace DotNetCleanTemplate.Api.Endpoints
{
    public class GetAllRolesEndpoint : EndpointWithoutRequest<Result<List<RoleDto>>>
    {
        private readonly IMediator _mediator;

        public GetAllRolesEndpoint(IMediator mediator)
        {
            _mediator = mediator;
        }

        public override void Configure()
        {
            Get("/administration/roles");
            Tags("Roles");
            Roles("Admin");
            Description(b =>
                b.WithSummary("Получить все роли").WithDescription("Возвращает список всех ролей")
            );
        }

        public override async Task HandleAsync(CancellationToken ct)
        {
            var result = await _mediator.Send(new GetAllRolesQuery(), ct);
            await SendAsync(result, cancellation: ct);
        }
    }
}

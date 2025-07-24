using DotNetCleanTemplate.Application.Configurations;
using DotNetCleanTemplate.Application.Features.Users;
using DotNetCleanTemplate.Shared.Common;
using DotNetCleanTemplate.Shared.DTOs;
using FastEndpoints;
using MediatR;
using Microsoft.Extensions.Options;

namespace DotNetCleanTemplate.Api.Endpoints
{
    public class GetRolesPaginatedEndpoint
        : Endpoint<GetRolesPaginatedQuery, Result<PaginatedResultDto<RoleDto>>>
    {
        private readonly IMediator _mediator;
        private readonly DefaultSettings _defaultSettings;

        public GetRolesPaginatedEndpoint(
            IMediator mediator,
            IOptions<DefaultSettings> defaultSettings
        )
        {
            _mediator = mediator;
            _defaultSettings = defaultSettings.Value;
        }

        public override void Configure()
        {
            Get("/administration/roles/paginated");
            Tags("Roles");
            Roles(_defaultSettings.DefaultAdminRole ?? "Admin");
            Description(b =>
                b.WithSummary("Получить роли с пейджингом")
                    .WithDescription("Возвращает список ролей с поддержкой пейджинга")
            );
        }

        public override async Task HandleAsync(GetRolesPaginatedQuery req, CancellationToken ct)
        {
            var result = await _mediator.Send(req, ct);
            await SendAsync(result, cancellation: ct);
        }
    }
}

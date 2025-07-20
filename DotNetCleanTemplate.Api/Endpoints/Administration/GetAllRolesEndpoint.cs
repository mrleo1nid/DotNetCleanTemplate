using DotNetCleanTemplate.Application.Configurations;
using DotNetCleanTemplate.Application.Features.Users;
using DotNetCleanTemplate.Shared.Common;
using DotNetCleanTemplate.Shared.DTOs;
using FastEndpoints;
using MediatR;
using Microsoft.Extensions.Options;

namespace DotNetCleanTemplate.Api.Endpoints
{
    public class GetAllRolesEndpoint : EndpointWithoutRequest<Result<List<RoleDto>>>
    {
        private readonly IMediator _mediator;
        private readonly DefaultSettings _defaultSettings;

        public GetAllRolesEndpoint(IMediator mediator, IOptions<DefaultSettings> defaultSettings)
        {
            _mediator = mediator;
            _defaultSettings = defaultSettings.Value;
        }

        public override void Configure()
        {
            Get("/administration/roles");
            Tags("Roles");
            Roles(_defaultSettings.DefaultAdminRole ?? "Admin");
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

using DotNetCleanTemplate.Application.Configurations;
using DotNetCleanTemplate.Application.Features.Users;
using DotNetCleanTemplate.Shared.Common;
using DotNetCleanTemplate.Shared.DTOs;
using FastEndpoints;
using MediatR;
using Microsoft.Extensions.Options;

namespace DotNetCleanTemplate.Api.Endpoints
{
    public class GetUsersWithRolesPaginatedEndpoint
        : Endpoint<GetUsersWithRolesPaginatedQuery, Result<PaginatedResultDto<UserWithRolesDto>>>
    {
        private readonly IMediator _mediator;
        private readonly DefaultSettings _defaultSettings;

        public GetUsersWithRolesPaginatedEndpoint(
            IMediator mediator,
            IOptions<DefaultSettings> defaultSettings
        )
        {
            _mediator = mediator;
            _defaultSettings = defaultSettings.Value;
        }

        public override void Configure()
        {
            Get("/administration/users/paginated");
            Tags("Users");
            Roles(_defaultSettings.DefaultAdminRole ?? "Admin");
            Description(b =>
                b.WithSummary("Получить пользователей с пейджингом")
                    .WithDescription(
                        "Возвращает список пользователей с их ролями с поддержкой пейджинга"
                    )
            );
        }

        public override async Task HandleAsync(
            GetUsersWithRolesPaginatedQuery req,
            CancellationToken ct
        )
        {
            var result = await _mediator.Send(req, ct);
            await SendAsync(result, cancellation: ct);
        }
    }
}

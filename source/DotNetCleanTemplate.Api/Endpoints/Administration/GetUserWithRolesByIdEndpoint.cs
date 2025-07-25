using DotNetCleanTemplate.Application.Configurations;
using DotNetCleanTemplate.Application.Features.Users;
using DotNetCleanTemplate.Shared.Common;
using DotNetCleanTemplate.Shared.DTOs;
using FastEndpoints;
using MediatR;
using Microsoft.Extensions.Options;

namespace DotNetCleanTemplate.Api.Endpoints
{
    public class GetUserWithRolesByIdEndpoint : EndpointWithoutRequest<Result<UserWithRolesDto>>
    {
        private readonly IMediator _mediator;
        private readonly DefaultSettings _defaultSettings;

        public GetUserWithRolesByIdEndpoint(
            IMediator mediator,
            IOptions<DefaultSettings> defaultSettings
        )
        {
            _mediator = mediator;
            _defaultSettings = defaultSettings.Value;
        }

        public override void Configure()
        {
            Get("/administration/users/{userId}/with-roles");
            Tags("Users");
            Roles(_defaultSettings.DefaultAdminRole ?? "Admin");
            Description(b =>
                b.WithSummary("Получить пользователя с ролями по ID")
                    .WithDescription(
                        "Возвращает пользователя с ролями в формате DTO по его идентификатору"
                    )
            );
        }

        public override async Task HandleAsync(CancellationToken ct)
        {
            var userId = Route<Guid>("userId");
            var query = new GetUserWithRolesByIdQuery { UserId = userId };
            var result = await _mediator.Send(query, ct);
            await SendAsync(result, cancellation: ct);
        }
    }
}

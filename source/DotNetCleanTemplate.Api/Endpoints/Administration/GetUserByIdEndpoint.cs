using DotNetCleanTemplate.Application.Configurations;
using DotNetCleanTemplate.Application.Features.Users;
using DotNetCleanTemplate.Domain.Entities;
using DotNetCleanTemplate.Shared.Common;
using FastEndpoints;
using MediatR;
using Microsoft.Extensions.Options;

namespace DotNetCleanTemplate.Api.Endpoints
{
    public class GetUserByIdEndpoint : EndpointWithoutRequest<Result<User>>
    {
        private readonly IMediator _mediator;
        private readonly DefaultSettings _defaultSettings;

        public GetUserByIdEndpoint(IMediator mediator, IOptions<DefaultSettings> defaultSettings)
        {
            _mediator = mediator;
            _defaultSettings = defaultSettings.Value;
        }

        public override void Configure()
        {
            Get("/administration/users/{userId}");
            Tags("Users");
            Roles(_defaultSettings.DefaultAdminRole ?? "Admin");
            Description(b =>
                b.WithSummary("Получить пользователя по ID")
                    .WithDescription("Возвращает пользователя с ролями по его идентификатору")
            );
        }

        public override async Task HandleAsync(CancellationToken ct)
        {
            var userId = Route<Guid>("userId");
            var query = new GetUserByIdQuery { UserId = userId };
            var result = await _mediator.Send(query, ct);
            await SendAsync(result, cancellation: ct);
        }
    }
}

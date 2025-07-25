using DotNetCleanTemplate.Application.Configurations;
using DotNetCleanTemplate.Application.Features.Users;
using DotNetCleanTemplate.Shared.Common;
using FastEndpoints;
using MediatR;
using Microsoft.Extensions.Options;

namespace DotNetCleanTemplate.Api.Endpoints
{
    public class DeleteUserEndpoint : EndpointWithoutRequest<Result<Unit>>
    {
        private readonly IMediator _mediator;
        private readonly DefaultSettings _defaultSettings;

        public DeleteUserEndpoint(IMediator mediator, IOptions<DefaultSettings> defaultSettings)
        {
            _mediator = mediator;
            _defaultSettings = defaultSettings.Value;
        }

        public override void Configure()
        {
            Delete("/administration/users/{userId}");
            Tags("Users");
            Roles(_defaultSettings.DefaultAdminRole ?? "Admin");
            Description(b =>
                b.WithSummary("Удалить пользователя")
                    .WithDescription("Удаляет пользователя из системы")
            );
        }

        public override async Task HandleAsync(CancellationToken ct)
        {
            var userId = Route<Guid>("userId");
            var command = new DeleteUserCommand { UserId = userId };
            var result = await _mediator.Send(command, ct);
            await SendAsync(result, cancellation: ct);
        }
    }
}

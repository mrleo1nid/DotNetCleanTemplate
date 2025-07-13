using DotNetCleanTemplate.Application.Features.Users;
using DotNetCleanTemplate.Shared.Common;
using DotNetCleanTemplate.Shared.DTOs;
using FastEndpoints;
using MediatR;

namespace DotNetCleanTemplate.Api.Endpoints
{
    public class GetAllUsersWithRolesEndpoint
        : EndpointWithoutRequest<Result<List<UserWithRolesDto>>>
    {
        private readonly IMediator _mediator;

        public GetAllUsersWithRolesEndpoint(IMediator mediator)
        {
            _mediator = mediator;
        }

        public override void Configure()
        {
            Get("/users");
            AllowAnonymous();
            Tags("Users");
            Description(b =>
                b.WithSummary("Получить всех пользователей и их роли")
                    .WithDescription("Возвращает список всех пользователей с их ролями")
            );
        }

        public override async Task HandleAsync(CancellationToken ct)
        {
            var result = await _mediator.Send(new GetAllUsersWithRolesQuery(), ct);
            await SendAsync(result, cancellation: ct);
        }
    }
}

using DotNetCleanTemplate.Shared.Common;
using MediatR;

namespace DotNetCleanTemplate.Application.Features.Users
{
    public class DeleteUserCommand : IRequest<Result<Unit>>
    {
        public Guid UserId { get; set; }
    }
}

using DotNetCleanTemplate.Domain.Entities;
using DotNetCleanTemplate.Shared.Common;
using MediatR;

namespace DotNetCleanTemplate.Application.Features.Users
{
    public class GetUserByIdQuery : IRequest<Result<User>>
    {
        public Guid UserId { get; set; }
    }
}

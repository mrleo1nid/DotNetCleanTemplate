using DotNetCleanTemplate.Shared.Common;
using DotNetCleanTemplate.Shared.DTOs;
using MediatR;

namespace DotNetCleanTemplate.Application.Features.Users.CreateUser;

public class CreateUserCommand : IRequest<Result<Guid>>
{
    public CreateUserDto Dto { get; set; } = null!;
}

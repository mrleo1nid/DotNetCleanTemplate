using DotNetCleanTemplate.Application.Features.Auth.RegisterUser;
using DotNetCleanTemplate.Shared.Common;
using DotNetCleanTemplate.Shared.DTOs;
using MediatR;

namespace DotNetCleanTemplate.Application.Features.Users.CreateUser;

public class CreateUserCommandHandler : IRequestHandler<CreateUserCommand, Result<Guid>>
{
    private readonly IMediator _mediator;

    public CreateUserCommandHandler(IMediator mediator)
    {
        _mediator = mediator;
    }

    public async Task<Result<Guid>> Handle(
        CreateUserCommand request,
        CancellationToken cancellationToken
    )
    {
        // Преобразуем CreateUserDto в RegisterUserDto
        var registerUserDto = new RegisterUserDto
        {
            UserName = request.Dto.UserName,
            Email = request.Dto.Email,
            Password = request.Dto.Password,
        };

        var registerCommand = new RegisterUserCommand { Dto = registerUserDto };
        return await _mediator.Send(registerCommand, cancellationToken);
    }
}

using DotNetCleanTemplate.Application.Interfaces;
using DotNetCleanTemplate.Domain.Entities;
using DotNetCleanTemplate.Domain.Services;
using DotNetCleanTemplate.Shared.Common;
using MediatR;

namespace DotNetCleanTemplate.Application.Features.Auth.RegisterUser
{
    public class RegisterUserCommandHandler : IRequestHandler<RegisterUserCommand, Result<Guid>>
    {
        private readonly IUserService _userService;
        private readonly IPasswordHasher _passwordHasher;

        public RegisterUserCommandHandler(IUserService userService, IPasswordHasher passwordHasher)
        {
            _userService = userService;
            _passwordHasher = passwordHasher;
        }

        public async Task<Result<Guid>> Handle(
            RegisterUserCommand request,
            CancellationToken cancellationToken
        )
        {
            var dto = request.Dto;
            var user = new User(
                new Domain.ValueObjects.User.UserName(dto.UserName),
                new Domain.ValueObjects.User.Email(dto.Email),
                new Domain.ValueObjects.User.PasswordHash(
                    _passwordHasher.HashPassword(dto.Password)
                )
            );
            var result = await _userService.CreateUserAsync(user, cancellationToken);
            if (!result.IsSuccess)
                return Result<Guid>.Failure(result.Errors);
            return Result<Guid>.Success(result.Value.Id);
        }
    }
}

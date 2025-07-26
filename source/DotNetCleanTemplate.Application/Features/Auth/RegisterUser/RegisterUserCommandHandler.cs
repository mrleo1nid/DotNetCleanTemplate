using DotNetCleanTemplate.Application.Interfaces;
using DotNetCleanTemplate.Domain.Entities;
using DotNetCleanTemplate.Domain.Factories.Entities;
using DotNetCleanTemplate.Domain.Services;
using DotNetCleanTemplate.Shared.Common;
using MediatR;

namespace DotNetCleanTemplate.Application.Features.Auth.RegisterUser
{
    public class RegisterUserCommandHandler : IRequestHandler<RegisterUserCommand, Result<Guid>>
    {
        private readonly IUserService _userService;
        private readonly IPasswordHasher _passwordHasher;
        private readonly IUserFactory _userFactory;

        public RegisterUserCommandHandler(
            IUserService userService,
            IPasswordHasher passwordHasher,
            IUserFactory userFactory
        )
        {
            _userService = userService;
            _passwordHasher = passwordHasher;
            _userFactory = userFactory;
        }

        public async Task<Result<Guid>> Handle(
            RegisterUserCommand request,
            CancellationToken cancellationToken
        )
        {
            var dto = request.Dto;
            var user = _userFactory.Create(
                dto.UserName,
                dto.Email,
                _passwordHasher.HashPassword(dto.Password)
            );
            var result = await _userService.CreateUserAsync(user, cancellationToken);
            if (!result.IsSuccess)
                return Result<Guid>.Failure(result.Errors);
            return Result<Guid>.Success(result.Value.Id);
        }
    }
}

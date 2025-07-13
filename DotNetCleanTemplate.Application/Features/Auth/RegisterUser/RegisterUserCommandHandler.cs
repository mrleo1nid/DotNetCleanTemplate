using AutoMapper;
using DotNetCleanTemplate.Application.Services;
using DotNetCleanTemplate.Domain.Entities;
using MediatR;

namespace DotNetCleanTemplate.Application.Features.Auth.RegisterUser
{
    public class RegisterUserCommandHandler : IRequestHandler<RegisterUserCommand, Guid>
    {
        private readonly UserService _userService;
        private readonly IMapper _mapper;

        public RegisterUserCommandHandler(UserService userService, IMapper mapper)
        {
            _userService = userService;
            _mapper = mapper;
        }

        public async Task<Guid> Handle(
            RegisterUserCommand request,
            CancellationToken cancellationToken
        )
        {
            var user = _mapper.Map<User>(request.Dto);
            var created = await _userService.CreateUserAsync(user, cancellationToken);
            return created.Id;
        }
    }
}

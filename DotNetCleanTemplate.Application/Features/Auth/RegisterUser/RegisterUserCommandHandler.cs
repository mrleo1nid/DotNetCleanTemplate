using DotNetCleanTemplate.Application.Interfaces;
using DotNetCleanTemplate.Domain.Entities;
using DotNetCleanTemplate.Shared.Common;
using MapsterMapper;
using MediatR;

namespace DotNetCleanTemplate.Application.Features.Auth.RegisterUser
{
    public class RegisterUserCommandHandler : IRequestHandler<RegisterUserCommand, Result<Guid>>
    {
        private readonly IUserService _userService;
        private readonly IMapper _mapper;

        public RegisterUserCommandHandler(IUserService userService, IMapper mapper)
        {
            _userService = userService;
            _mapper = mapper;
        }

        public async Task<Result<Guid>> Handle(
            RegisterUserCommand request,
            CancellationToken cancellationToken
        )
        {
            var user = _mapper.Map<User>(request.Dto);
            var result = await _userService.CreateUserAsync(user, cancellationToken);
            if (!result.IsSuccess)
                return Result<Guid>.Failure(result.Errors);
            return Result<Guid>.Success(result.Value.Id);
        }
    }
}

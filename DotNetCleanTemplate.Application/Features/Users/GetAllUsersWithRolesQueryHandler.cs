using DotNetCleanTemplate.Application.Interfaces;
using DotNetCleanTemplate.Shared.Common;
using DotNetCleanTemplate.Shared.DTOs;
using MapsterMapper;
using MediatR;

namespace DotNetCleanTemplate.Application.Features.Users
{
    public class GetAllUsersWithRolesQueryHandler
        : IRequestHandler<GetAllUsersWithRolesQuery, Result<List<UserWithRolesDto>>>
    {
        private readonly IUserService _userService;
        private readonly IMapper _mapper;

        public GetAllUsersWithRolesQueryHandler(IUserService userService, IMapper mapper)
        {
            _userService = userService;
            _mapper = mapper;
        }

        public async Task<Result<List<UserWithRolesDto>>> Handle(
            GetAllUsersWithRolesQuery request,
            CancellationToken cancellationToken
        )
        {
            var result = await _userService.GetAllUsersWithRolesAsync(cancellationToken);
            if (!result.IsSuccess)
                return Result<List<UserWithRolesDto>>.Failure(result.Errors);
            var mapped = _mapper.Map<List<UserWithRolesDto>>(result.Value);
            return Result<List<UserWithRolesDto>>.Success(mapped);
        }
    }
}

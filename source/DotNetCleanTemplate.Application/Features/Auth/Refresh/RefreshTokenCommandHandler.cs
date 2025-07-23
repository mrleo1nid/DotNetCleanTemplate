using DotNetCleanTemplate.Domain.Repositories;
using DotNetCleanTemplate.Domain.Services;
using DotNetCleanTemplate.Shared.Common;
using DotNetCleanTemplate.Shared.DTOs;
using MediatR;

namespace DotNetCleanTemplate.Application.Features.Auth.Refresh
{
    public class RefreshTokenCommandHandler
        : IRequestHandler<RefreshTokenCommand, Result<RefreshTokenResponseDto>>
    {
        private readonly ITokenService _tokenService;
        private readonly IUserRepository _userRepository;

        public RefreshTokenCommandHandler(
            ITokenService tokenService,
            IUserRepository userRepository
        )
        {
            _tokenService = tokenService;
            _userRepository = userRepository;
        }

        public async Task<Result<RefreshTokenResponseDto>> Handle(
            RefreshTokenCommand request,
            CancellationToken cancellationToken
        )
        {
            try
            {
                var refreshToken = await _tokenService.ValidateRefreshTokenAsync(
                    request.RefreshToken,
                    cancellationToken
                );
                var user = await _userRepository.GetUserWithRolesAsync(
                    refreshToken.UserId,
                    cancellationToken
                );
                if (user == null)
                    return Result<RefreshTokenResponseDto>.Failure(
                        "User.NotFound",
                        "User not found for refresh token."
                    );

                var accessToken = _tokenService.GenerateAccessToken(user);
                var newRefreshToken = await _tokenService.CreateAndStoreRefreshTokenAsync(
                    user,
                    refreshToken.CreatedByIp,
                    cancellationToken
                );

                var dto = new RefreshTokenResponseDto
                {
                    RefreshToken = newRefreshToken.Token,
                    AccessToken = accessToken,
                    Expires = newRefreshToken.Expires,
                };
                return Result<RefreshTokenResponseDto>.Success(dto);
            }
            catch (Exception ex)
            {
                return Result<RefreshTokenResponseDto>.Failure("RefreshToken.Error", ex.Message);
            }
        }
    }
}

using DotNetCleanTemplate.Domain.Entities;
using DotNetCleanTemplate.Domain.Repositories;
using DotNetCleanTemplate.Domain.Services;
using DotNetCleanTemplate.Shared.Common;
using DotNetCleanTemplate.Shared.DTOs;
using MediatR;

namespace DotNetCleanTemplate.Application.Features.Auth.Login
{
    public class LoginCommandHandler : IRequestHandler<LoginCommand, Result<LoginResponseDto>>
    {
        private readonly IUserRepository _userRepository;
        private readonly ITokenService _tokenService;

        public LoginCommandHandler(IUserRepository userRepository, ITokenService tokenService)
        {
            _userRepository = userRepository;
            _tokenService = tokenService;
        }

        public async Task<Result<LoginResponseDto>> Handle(
            LoginCommand request,
            CancellationToken cancellationToken
        )
        {
            var user = await _userRepository.FindByEmailAsync(request.Dto.Email, cancellationToken);
            if (user == null)
                return Result<LoginResponseDto>.Failure(
                    "Auth.InvalidCredentials",
                    "Invalid email or password."
                );

            // Пример: простая проверка пароля (в реальном проекте — через hash)
            if (user.PasswordHash.Value != request.Dto.Password)
                return Result<LoginResponseDto>.Failure(
                    "Auth.InvalidCredentials",
                    "Invalid email or password."
                );

            var accessToken = _tokenService.GenerateAccessToken(user);
            var refreshToken = await _tokenService.CreateAndStoreRefreshTokenAsync(
                user,
                "login",
                cancellationToken
            );

            var dto = new LoginResponseDto
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken.Token,
                RefreshTokenExpires = refreshToken.Expires,
            };
            return Result<LoginResponseDto>.Success(dto);
        }
    }
}

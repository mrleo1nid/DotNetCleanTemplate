using DotNetCleanTemplate.Application.Interfaces;
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
        private readonly IPasswordHasher _passwordHasher;
        private readonly IUserLockoutService _userLockoutService;

        public LoginCommandHandler(
            IUserRepository userRepository,
            ITokenService tokenService,
            IPasswordHasher passwordHasher,
            IUserLockoutService userLockoutService
        )
        {
            _userRepository = userRepository;
            _tokenService = tokenService;
            _passwordHasher = passwordHasher;
            _userLockoutService = userLockoutService;
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

            // Проверяем, не заблокирован ли пользователь
            var lockoutCheck = await _userLockoutService.CheckUserLockoutAsync(
                user.Id,
                cancellationToken
            );
            if (!lockoutCheck.IsSuccess)
                return Result<LoginResponseDto>.Failure(lockoutCheck.Errors);

            // Проверка пароля через IPasswordHasher
            if (!_passwordHasher.VerifyPassword(user.PasswordHash.Value, request.Dto.Password))
            {
                // Записываем неудачную попытку входа
                await _userLockoutService.RecordFailedLoginAttemptAsync(user.Id, cancellationToken);

                return Result<LoginResponseDto>.Failure(
                    "Auth.InvalidCredentials",
                    "Invalid email or password."
                );
            }

            // Успешный вход - очищаем блокировку
            await _userLockoutService.RecordSuccessfulLoginAsync(user.Id, cancellationToken);

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

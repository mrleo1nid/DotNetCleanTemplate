using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using DotNetCleanTemplate.Domain.Entities;
using DotNetCleanTemplate.Domain.Repositories;
using DotNetCleanTemplate.Domain.Services;
using DotNetCleanTemplate.Infrastructure.Configurations;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace DotNetCleanTemplate.Infrastructure.Services
{
    public class TokenService : ITokenService
    {
        private readonly JwtSettings _jwtSettings;
        private readonly IRefreshTokenRepository _refreshTokenRepository;
        private readonly IUnitOfWork _unitOfWork;

        public TokenService(
            IOptions<JwtSettings> jwtOptions,
            IRefreshTokenRepository refreshTokenRepository,
            IUnitOfWork unitOfWork
        )
        {
            _jwtSettings = jwtOptions.Value;
            _refreshTokenRepository = refreshTokenRepository;
            _unitOfWork = unitOfWork;
        }

        public string GenerateAccessToken(User user)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Email, user.Email.Value),
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.Key));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _jwtSettings.Issuer,
                audience: _jwtSettings.Audience,
                claims: claims,
                expires: DateTime.Now.AddMinutes(_jwtSettings.AccessTokenExpirationMinutes),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public string GenerateRefreshToken()
        {
            var randomNumber = new byte[32];
            using var rng = System.Security.Cryptography.RandomNumberGenerator.Create();
            rng.GetBytes(randomNumber);
            return Convert.ToBase64String(randomNumber);
        }

        public async Task<RefreshToken> CreateAndStoreRefreshTokenAsync(
            User user,
            string createdByIp,
            CancellationToken cancellationToken = default
        )
        {
            var refreshTokenString = GenerateRefreshToken();
            var refreshToken = new RefreshToken(
                refreshTokenString,
                DateTime.UtcNow.AddDays(_jwtSettings.RefreshTokenExpirationDays),
                user.Id,
                createdByIp
            );
            await _refreshTokenRepository.AddAsync(refreshToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return refreshToken;
        }

        public async Task<RefreshToken> ValidateRefreshTokenAsync(
            string token,
            CancellationToken cancellationToken = default
        )
        {
            var refreshToken = await _refreshTokenRepository.FindByTokenAsync(
                token,
                cancellationToken
            );
            if (refreshToken == null || !refreshToken.IsActive)
                throw new InvalidOperationException("Refresh token is invalid or revoked.");
            return refreshToken;
        }
    }
}

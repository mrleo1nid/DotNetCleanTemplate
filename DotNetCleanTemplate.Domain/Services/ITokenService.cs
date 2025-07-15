using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DotNetCleanTemplate.Domain.Entities;

namespace DotNetCleanTemplate.Domain.Services
{
    public interface ITokenService
    {
        string GenerateAccessToken(User user);
        string GenerateRefreshToken();
        Task<RefreshToken> CreateAndStoreRefreshTokenAsync(
            User user,
            string createdByIp,
            CancellationToken cancellationToken = default
        );
        Task<RefreshToken> ValidateRefreshTokenAsync(
            string token,
            CancellationToken cancellationToken = default
        );
    }
}

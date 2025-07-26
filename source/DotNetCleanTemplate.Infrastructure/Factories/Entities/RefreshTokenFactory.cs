using DotNetCleanTemplate.Domain.Factories.Entities;
using RefreshTokenEntity = DotNetCleanTemplate.Domain.Entities.RefreshToken;

namespace DotNetCleanTemplate.Infrastructure.Factories.Entities
{
    public class RefreshTokenFactory : IRefreshTokenFactory
    {
        public RefreshTokenEntity Create(Guid userId, string token, DateTime expiresAt)
        {
            return new RefreshTokenEntity(token, expiresAt, userId, "127.0.0.1");
        }
    }
}

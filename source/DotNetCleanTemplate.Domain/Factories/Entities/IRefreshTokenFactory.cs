using RefreshTokenEntity = DotNetCleanTemplate.Domain.Entities.RefreshToken;

namespace DotNetCleanTemplate.Domain.Factories.Entities
{
    public interface IRefreshTokenFactory : IFactory
    {
        RefreshTokenEntity Create(Guid userId, string token, DateTime expiresAt);
    }
}

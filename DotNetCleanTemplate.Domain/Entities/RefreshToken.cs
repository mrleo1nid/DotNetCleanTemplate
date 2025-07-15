using DotNetCleanTemplate.Domain.Common;

namespace DotNetCleanTemplate.Domain.Entities
{
    public class RefreshToken : Entity<Guid>
    {
        public string Token { get; private set; }
        public DateTime Expires { get; private set; }
        public Guid UserId { get; private set; }
        public User User { get; private set; } = default!;
        public string CreatedByIp { get; private set; }
        public DateTime? RevokedAt { get; private set; }
        public string? RevokedByIp { get; private set; }
        public string? ReplacedByToken { get; private set; }

        public bool IsActive => RevokedAt == null && !IsExpired;
        public bool IsExpired => DateTime.UtcNow >= Expires;

        // Для EF Core
#pragma warning disable CS8618
        protected RefreshToken() { }
#pragma warning restore CS8618

        public RefreshToken(string token, DateTime expires, Guid userId, string createdByIp)
        {
            Token = token ?? throw new ArgumentNullException(nameof(token));
            Expires = expires;
            UserId = userId;
            CreatedByIp = createdByIp ?? throw new ArgumentNullException(nameof(createdByIp));
        }

        public void Revoke(string revokedByIp)
        {
            if (IsExpired)
                throw new InvalidOperationException("Token is already expired.");
            if (RevokedAt != null)
                throw new InvalidOperationException("Token is already revoked.");

            RevokedAt = DateTime.UtcNow;
            RevokedByIp = revokedByIp ?? throw new ArgumentNullException(nameof(revokedByIp));
        }

        public void Replace(string newToken)
        {
            if (string.IsNullOrWhiteSpace(newToken))
                throw new ArgumentNullException(nameof(newToken));
            ReplacedByToken = newToken;
        }
    }
}

using System;
using System.Security.Cryptography;
using DotNetCleanTemplate.Domain.Services;

namespace DotNetCleanTemplate.Infrastructure.Services
{
    public class PasswordHasher : IPasswordHasher
    {
        private const int SaltSize = 16;
        private const int HashSize = 32;
        private const int Iterations = 100_000;

        public string HashPassword(string password)
        {
            var salt = RandomNumberGenerator.GetBytes(SaltSize);
            var hash = new Rfc2898DeriveBytes(
                password,
                salt,
                Iterations,
                HashAlgorithmName.SHA256
            ).GetBytes(HashSize);
            return $"{Convert.ToBase64String(salt)}.{Convert.ToBase64String(hash)}";
        }

        public bool VerifyPassword(string hash, string password)
        {
            if (string.IsNullOrWhiteSpace(hash) || string.IsNullOrWhiteSpace(password))
                return false;
            var parts = hash.Split('.');
            if (parts.Length != 2)
                return false;
            var salt = Convert.FromBase64String(parts[0]);
            var expectedHash = Convert.FromBase64String(parts[1]);
            var actualHash = new Rfc2898DeriveBytes(
                password,
                salt,
                Iterations,
                HashAlgorithmName.SHA256
            ).GetBytes(HashSize);
            return CryptographicOperations.FixedTimeEquals(expectedHash, actualHash);
        }
    }
}

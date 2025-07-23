namespace DotNetCleanTemplate.Domain.Services
{
    public interface IPasswordHasher
    {
        string HashPassword(string password);
        bool VerifyPassword(string hash, string password);
    }
}

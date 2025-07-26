using DotNetCleanTemplate.Domain.Factories.User;
using DotNetCleanTemplate.Domain.ValueObjects.User;

namespace DotNetCleanTemplate.Infrastructure.Factories.User
{
    public class PasswordHashFactory : IPasswordHashFactory
    {
        public PasswordHash Create(string hashedPassword)
        {
            return new PasswordHash(hashedPassword);
        }
    }
}

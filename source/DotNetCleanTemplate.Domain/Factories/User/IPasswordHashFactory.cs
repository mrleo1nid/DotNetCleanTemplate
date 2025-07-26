using DotNetCleanTemplate.Domain.ValueObjects.User;

namespace DotNetCleanTemplate.Domain.Factories.User
{
    public interface IPasswordHashFactory : IFactory
    {
        PasswordHash Create(string hashedPassword);
    }
}

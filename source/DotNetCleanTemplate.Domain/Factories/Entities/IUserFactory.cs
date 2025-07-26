using DotNetCleanTemplate.Domain.Factories.User;
using UserEntity = DotNetCleanTemplate.Domain.Entities.User;

namespace DotNetCleanTemplate.Domain.Factories.Entities
{
    public interface IUserFactory : IFactory
    {
        UserEntity Create(string userName, string email, string passwordHash);
    }
}

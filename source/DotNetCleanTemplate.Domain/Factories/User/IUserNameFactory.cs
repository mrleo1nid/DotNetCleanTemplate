using DotNetCleanTemplate.Domain.ValueObjects.User;

namespace DotNetCleanTemplate.Domain.Factories.User
{
    public interface IUserNameFactory : IFactory
    {
        UserName Create(string userName);
    }
}

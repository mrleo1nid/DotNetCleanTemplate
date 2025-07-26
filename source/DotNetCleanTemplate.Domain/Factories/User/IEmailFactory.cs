using DotNetCleanTemplate.Domain.ValueObjects.User;

namespace DotNetCleanTemplate.Domain.Factories.User
{
    public interface IEmailFactory : IFactory
    {
        Email Create(string email);
    }
}

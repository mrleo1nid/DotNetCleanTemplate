using DotNetCleanTemplate.Domain.Factories.User;
using DotNetCleanTemplate.Domain.ValueObjects.User;

namespace DotNetCleanTemplate.Infrastructure.Factories.User
{
    public class UserNameFactory : IUserNameFactory
    {
        public UserName Create(string userName)
        {
            return new UserName(userName);
        }
    }
}

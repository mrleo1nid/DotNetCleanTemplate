using DotNetCleanTemplate.Domain.Factories.User;
using DotNetCleanTemplate.Domain.ValueObjects.User;

namespace DotNetCleanTemplate.Infrastructure.Factories.User
{
    public class EmailFactory : IEmailFactory
    {
        public Email Create(string email)
        {
            return new Email(email);
        }
    }
}

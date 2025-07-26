using DotNetCleanTemplate.Domain.Factories.Entities;
using DotNetCleanTemplate.Domain.Factories.User;
using DotNetCleanTemplate.Domain.ValueObjects.User;
using UserEntity = DotNetCleanTemplate.Domain.Entities.User;

namespace DotNetCleanTemplate.Infrastructure.Factories.Entities
{
    public class UserFactory : IUserFactory
    {
        private readonly IEmailFactory _emailFactory;
        private readonly IUserNameFactory _userNameFactory;
        private readonly IPasswordHashFactory _passwordHashFactory;

        public UserFactory(
            IEmailFactory emailFactory,
            IUserNameFactory userNameFactory,
            IPasswordHashFactory passwordHashFactory
        )
        {
            _emailFactory = emailFactory;
            _userNameFactory = userNameFactory;
            _passwordHashFactory = passwordHashFactory;
        }

        public UserEntity Create(string userName, string email, string passwordHash)
        {
            var userNameValue = _userNameFactory.Create(userName);
            var emailValue = _emailFactory.Create(email);
            var passwordHashValue = _passwordHashFactory.Create(passwordHash);

            return new UserEntity(userNameValue, emailValue, passwordHashValue);
        }
    }
}

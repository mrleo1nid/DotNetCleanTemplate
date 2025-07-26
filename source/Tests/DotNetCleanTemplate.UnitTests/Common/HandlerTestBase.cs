using DotNetCleanTemplate.Domain.Entities;
using DotNetCleanTemplate.Domain.Factories.Entities;
using DotNetCleanTemplate.Infrastructure.Factories.Entities;
using DotNetCleanTemplate.Infrastructure.Factories.User;
using DotNetCleanTemplate.Infrastructure.Services;

namespace DotNetCleanTemplate.UnitTests.Common
{
    public abstract class HandlerTestBase : TestBase
    {
        protected static User CreateTestUser(
            string? email = null,
            string? password = null,
            string? userName = null
        )
        {
            var passwordHasher = new PasswordHasher();
            var emailFactory = new EmailFactory();
            var userNameFactory = new UserNameFactory();
            var passwordHashFactory = new PasswordHashFactory();
            var userFactory = new UserFactory(emailFactory, userNameFactory, passwordHashFactory);

            return userFactory.Create(
                userName ?? "TestUser",
                email ?? $"test{Guid.NewGuid()}@example.com",
                passwordHasher.HashPassword(password ?? "12345678901234567890")
            );
        }

        protected static string CreateValidPassword()
        {
            return "12345678901234567890";
        }

        protected static string CreateInvalidPassword()
        {
            return "123";
        }

        protected static string CreateValidEmail()
        {
            return $"test{Guid.NewGuid()}@example.com";
        }

        protected static string CreateInvalidEmail()
        {
            return "invalid-email";
        }
    }
}

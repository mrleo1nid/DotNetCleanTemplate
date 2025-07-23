using DotNetCleanTemplate.Domain.Entities;
using DotNetCleanTemplate.Domain.ValueObjects.User;
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
            return new User(
                new UserName(userName ?? "TestUser"),
                new Email(email ?? $"test{Guid.NewGuid()}@example.com"),
                new PasswordHash(passwordHasher.HashPassword(password ?? "12345678901234567890"))
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

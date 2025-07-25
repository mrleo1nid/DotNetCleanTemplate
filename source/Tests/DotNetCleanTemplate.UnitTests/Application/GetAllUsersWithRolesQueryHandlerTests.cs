using DotNetCleanTemplate.Application.Features.Users;
using DotNetCleanTemplate.Application.Mapping;
using DotNetCleanTemplate.Application.Services;
using DotNetCleanTemplate.Domain.Entities;
using DotNetCleanTemplate.Domain.ValueObjects.Role;
using DotNetCleanTemplate.Domain.ValueObjects.User;
using DotNetCleanTemplate.Infrastructure.Persistent;
using DotNetCleanTemplate.Infrastructure.Persistent.Repositories;
using DotNetCleanTemplate.UnitTests.Common;
using Mapster;
using MapsterMapper;

namespace DotNetCleanTemplate.UnitTests.Application
{
    public class GetAllUsersWithRolesQueryHandlerTests : ServiceTestBase
    {
        private static GetAllUsersWithRolesQueryHandler CreateHandler(AppDbContext context)
        {
            var userService = ServiceTestBase.CreateUserService(context);

            var config = new TypeAdapterConfig();
            new UserMappingConfig().Register(config);
            var mapper = new Mapper(config);

            return new GetAllUsersWithRolesQueryHandler(userService, mapper);
        }

        [Fact]
        public async Task Returns_Empty_When_No_Users()
        {
            using var context = CreateDbContext();
            var handler = CreateHandler(context);
            var result = await handler.Handle(
                new GetAllUsersWithRolesQuery(),
                CancellationToken.None
            );
            Assert.NotNull(result);
            Assert.False(result.IsSuccess);
            Assert.Equal("User.NotFound", result.Errors[0].Code);
        }

        [Fact]
        public async Task Returns_Users_With_Roles()
        {
            using var context = CreateDbContext();
            // Arrange: создать роли и пользователей
            var roleAdmin = new Role(new RoleName("Admin"));
            var roleUser = new Role(new RoleName("User"));
            context.Roles.AddRange(roleAdmin, roleUser);
            await context.SaveChangesAsync();

            var user1 = new User(
                new UserName("Alice"),
                new Email("alice@example.com"),
                new PasswordHash("12345678901234567890")
            );
            var user2 = new User(
                new UserName("Bob"),
                new Email("bob@example.com"),
                new PasswordHash("12345678901234567890")
            );
            user1.AssignRole(roleAdmin);
            user2.AssignRole(roleUser);
            context.Users.AddRange(user1, user2);
            await context.SaveChangesAsync();
            var handler = CreateHandler(context);
            // Act
            var result = await handler.Handle(
                new GetAllUsersWithRolesQuery(),
                CancellationToken.None
            );
            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(2, result.Value.Count);
            var alice = result.Value.FirstOrDefault(u => u.UserName == "Alice");
            var bob = result.Value.FirstOrDefault(u => u.UserName == "Bob");
            Assert.NotNull(alice);
            Assert.Single(alice!.Roles);
            Assert.Equal("Admin", alice.Roles[0].Name);
            Assert.NotNull(bob);
            Assert.Single(bob!.Roles);
            Assert.Equal("User", bob.Roles[0].Name);
        }
    }
}

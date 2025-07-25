using DotNetCleanTemplate.Application.Features.Auth.RegisterUser;
using DotNetCleanTemplate.Application.Services;
using DotNetCleanTemplate.Infrastructure.Persistent;
using DotNetCleanTemplate.Infrastructure.Persistent.Repositories;
using DotNetCleanTemplate.Infrastructure.Services;
using DotNetCleanTemplate.Shared.DTOs;
using DotNetCleanTemplate.UnitTests.Common;

namespace DotNetCleanTemplate.UnitTests.Application
{
    public class RegisterUserCommandHandlerTests : ServiceTestBase
    {
        private static RegisterUserDto CreateDto(string? email = null)
        {
            return new RegisterUserDto
            {
                UserName = "TestUser",
                Email = email ?? $"test{Guid.NewGuid()}@example.com",
                Password = "12345678901234567890",
            };
        }

        private static RegisterUserCommandHandler CreateHandler(AppDbContext context)
        {
            var userService = CreateUserService(context);
            var passwordHasher = new DotNetCleanTemplate.Infrastructure.Services.PasswordHasher();
            return new RegisterUserCommandHandler(userService, passwordHasher);
        }

        [Fact]
        public async Task RegisterUser_Success()
        {
            using var context = CreateDbContext();
            var handler = CreateHandler(context);
            var dto = CreateDto();
            var command = new RegisterUserCommand { Dto = dto };
            var result = await handler.Handle(command, CancellationToken.None);
            Assert.True(result.IsSuccess);
            Assert.NotEqual(Guid.Empty, result.Value);
            var user = await context.Users.FindAsync(result.Value);
            Assert.NotNull(user);
            Assert.Equal(dto.Email, user!.Email.Value);
        }

        [Fact]
        public async Task RegisterUser_DuplicateEmail_Throws()
        {
            using var context = CreateDbContext();
            var handler = CreateHandler(context);
            var dto = CreateDto("duplicate@example.com");
            var command = new RegisterUserCommand { Dto = dto };
            await handler.Handle(command, CancellationToken.None);
            // Попытка зарегистрировать с тем же email
            var duplicateCommand = new RegisterUserCommand { Dto = dto };
            var duplicateResult = await handler.Handle(duplicateCommand, CancellationToken.None);
            Assert.False(duplicateResult.IsSuccess);
            Assert.Contains(duplicateResult.Errors, e => e.Code == "User.AlreadyExists");
        }

        [Fact]
        public void RegisterUserCommand_CanBeCreated()
        {
            var dto = new RegisterUserDto
            {
                UserName = "TestUser",
                Email = "test@example.com",
                Password = "password123",
            };
            var command = new RegisterUserCommand { Dto = dto };
            Assert.NotNull(command);
            Assert.Equal(dto, command.Dto);
        }
    }
}

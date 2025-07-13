using AutoMapper;
using DotNetCleanTemplate.Application.Features.Auth.RegisterUser;
using DotNetCleanTemplate.Application.Mapping;
using DotNetCleanTemplate.Application.Services;
using DotNetCleanTemplate.Infrastructure.Persistent;
using DotNetCleanTemplate.Infrastructure.Persistent.Repositories;
using DotNetCleanTemplate.Shared.DTOs;

namespace ApplicationTests
{
    public class RegisterUserCommandHandlerTests : TestBase
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
            var userRepository = new UserRepository(context);
            var unitOfWork = new UnitOfWork(context);
            var userService = new UserService(userRepository, unitOfWork);

            var config = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile<RegisterUserMappingProfile>();
            });
            var mapper = config.CreateMapper();

            return new RegisterUserCommandHandler(userService, mapper);
        }

        [Fact]
        public async Task RegisterUser_Success()
        {
            using var context = CreateDbContext();
            var handler = CreateHandler(context);
            var dto = CreateDto();
            var command = new RegisterUserCommand { Dto = dto };
            var userId = await handler.Handle(command, CancellationToken.None);
            Assert.NotEqual(Guid.Empty, userId);
            var user = await context.Users.FindAsync(userId);
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
            await Assert.ThrowsAsync<Exception>(async () =>
            {
                await handler.Handle(duplicateCommand, CancellationToken.None);
            });
        }
    }
}

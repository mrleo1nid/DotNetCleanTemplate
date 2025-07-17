using DotNetCleanTemplate.Application.Features.Users;
using DotNetCleanTemplate.Application.Interfaces;
using DotNetCleanTemplate.Shared.Common;
using DotNetCleanTemplate.Shared.DTOs;
using MediatR;
using Moq;

namespace ApplicationTests
{
    public class AssignRoleToUserCommandHandlerTests
    {
        [Fact]
        public async Task Handle_ReturnsSuccess_WhenServiceSucceeds()
        {
            // Arrange
            var mockService = new Mock<IUserService>();
            mockService
                .Setup(s =>
                    s.AssignRoleToUserAsync(
                        It.IsAny<Guid>(),
                        It.IsAny<Guid>(),
                        It.IsAny<CancellationToken>()
                    )
                )
                .ReturnsAsync(Result<Unit>.Success());
            var handler = new AssignRoleToUserCommandHandler(mockService.Object);
            var dto = new AssignRoleToUserDto { UserId = Guid.NewGuid(), RoleId = Guid.NewGuid() };
            var command = new AssignRoleToUserCommand { Dto = dto };

            // Act
            var result = await handler.Handle(command, default);

            // Assert
            Assert.True(result.IsSuccess);
        }

        [Fact]
        public async Task Handle_ReturnsFailure_WhenServiceFails()
        {
            // Arrange
            var mockService = new Mock<IUserService>();
            mockService
                .Setup(s =>
                    s.AssignRoleToUserAsync(
                        It.IsAny<Guid>(),
                        It.IsAny<Guid>(),
                        It.IsAny<CancellationToken>()
                    )
                )
                .ReturnsAsync(Result<Unit>.Failure("error", "fail"));
            var handler = new AssignRoleToUserCommandHandler(mockService.Object);
            var dto = new AssignRoleToUserDto { UserId = Guid.NewGuid(), RoleId = Guid.NewGuid() };
            var command = new AssignRoleToUserCommand { Dto = dto };

            // Act
            var result = await handler.Handle(command, default);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.NotEmpty(result.Errors);
        }
    }
}

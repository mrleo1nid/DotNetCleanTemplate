using DotNetCleanTemplate.Application.Features.Users;
using DotNetCleanTemplate.Application.Interfaces;
using DotNetCleanTemplate.Shared.Common;
using MediatR;
using Moq;

namespace DotNetCleanTemplate.UnitTests.Application
{
    public class DeleteUserCommandHandlerTests
    {
        [Fact]
        public async Task Handle_ReturnsSuccess_WhenServiceSucceeds()
        {
            // Arrange
            var mockService = new Mock<IUserService>();
            mockService
                .Setup(s => s.DeleteUserAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result<Unit>.Success());
            var handler = new DeleteUserCommandHandler(mockService.Object);
            var command = new DeleteUserCommand { UserId = Guid.NewGuid() };

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
                .Setup(s => s.DeleteUserAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result<Unit>.Failure("error", "fail"));
            var handler = new DeleteUserCommandHandler(mockService.Object);
            var command = new DeleteUserCommand { UserId = Guid.NewGuid() };

            // Act
            var result = await handler.Handle(command, default);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.NotEmpty(result.Errors);
        }
    }
}

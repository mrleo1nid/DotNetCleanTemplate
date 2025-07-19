using DotNetCleanTemplate.Application.Features.Users;
using DotNetCleanTemplate.Application.Interfaces;
using DotNetCleanTemplate.Domain.Entities;
using DotNetCleanTemplate.Shared.Common;
using Moq;

namespace DotNetCleanTemplate.UnitTests.Application
{
    public class GetAllRolesQueryHandlerTests
    {
        [Fact]
        public async Task Handle_ReturnsSuccess_WhenRolesExist()
        {
            // Arrange
            var roles = new List<Role> { new Role(new("Admin")), new Role(new("User")) };
            var mockService = new Mock<IRoleService>();
            mockService
                .Setup(s => s.GetAllRolesAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result<List<Role>>.Success(roles));
            var handler = new GetAllRolesQueryHandler(mockService.Object);

            // Act
            var result = await handler.Handle(new GetAllRolesQuery(), default);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(2, result.Value.Count);
            Assert.Contains(result.Value, r => r.Name == "Admin");
        }

        [Fact]
        public async Task Handle_ReturnsFailure_WhenServiceFails()
        {
            // Arrange
            var mockService = new Mock<IRoleService>();
            mockService
                .Setup(s => s.GetAllRolesAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result<List<Role>>.Failure("error", "fail"));
            var handler = new GetAllRolesQueryHandler(mockService.Object);

            // Act
            var result = await handler.Handle(new GetAllRolesQuery(), default);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.NotEmpty(result.Errors);
        }
    }
}

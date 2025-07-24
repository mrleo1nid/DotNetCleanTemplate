using DotNetCleanTemplate.Application.Features.Users;
using DotNetCleanTemplate.Application.Interfaces;
using DotNetCleanTemplate.Domain.Entities;
using DotNetCleanTemplate.Domain.ValueObjects.Role;
using DotNetCleanTemplate.Shared.Common;
using DotNetCleanTemplate.Shared.DTOs;
using MediatR;
using Moq;

namespace DotNetCleanTemplate.UnitTests.Application
{
    public class GetRolesPaginatedQueryHandlerTests
    {
        private readonly Mock<IRoleService> _roleServiceMock;
        private readonly GetRolesPaginatedQueryHandler _handler;

        public GetRolesPaginatedQueryHandlerTests()
        {
            _roleServiceMock = new Mock<IRoleService>();
            _handler = new GetRolesPaginatedQueryHandler(_roleServiceMock.Object);
        }

        [Fact]
        public async Task Handle_ValidRequest_ReturnsPaginatedResult()
        {
            // Arrange
            var query = new GetRolesPaginatedQuery { Page = 1, PageSize = 10 };
            var roles = new List<Role> { new Role(new RoleName("Admin")) };

            _roleServiceMock
                .Setup(x => x.GetRolesPaginatedAsync(1, 10, It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result<(List<Role> Roles, int TotalCount)>.Success((roles, 1)));

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.NotNull(result.Value);
            Assert.Single(result.Value.Items);
            Assert.Equal(1, result.Value.TotalCount);
            Assert.Equal(1, result.Value.TotalPages);
            Assert.False(result.Value.HasPreviousPage);
            Assert.False(result.Value.HasNextPage);
        }

        [Fact]
        public async Task Handle_ServiceFailure_ReturnsFailure()
        {
            // Arrange
            var query = new GetRolesPaginatedQuery { Page = 1, PageSize = 10 };
            _roleServiceMock
                .Setup(x => x.GetRolesPaginatedAsync(1, 10, It.IsAny<CancellationToken>()))
                .ReturnsAsync(
                    Result<(List<Role> Roles, int TotalCount)>.Failure(
                        "Test.Error",
                        "Test error message"
                    )
                );

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.NotEmpty(result.Errors);
        }
    }
}

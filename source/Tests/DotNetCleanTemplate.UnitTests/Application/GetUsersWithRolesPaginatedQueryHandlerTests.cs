using DotNetCleanTemplate.Application.Features.Users;
using DotNetCleanTemplate.Application.Interfaces;
using DotNetCleanTemplate.Domain.Entities;
using DotNetCleanTemplate.Domain.ValueObjects.User;
using DotNetCleanTemplate.Shared.Common;
using DotNetCleanTemplate.Shared.DTOs;
using MapsterMapper;
using Moq;

namespace DotNetCleanTemplate.UnitTests.Application
{
    public class GetUsersWithRolesPaginatedQueryHandlerTests
    {
        private readonly Mock<IUserService> _userServiceMock;
        private readonly Mock<IMapper> _mapperMock;
        private readonly GetUsersWithRolesPaginatedQueryHandler _handler;

        public GetUsersWithRolesPaginatedQueryHandlerTests()
        {
            _userServiceMock = new Mock<IUserService>();
            _mapperMock = new Mock<IMapper>();
            _handler = new GetUsersWithRolesPaginatedQueryHandler(
                _userServiceMock.Object,
                _mapperMock.Object
            );
        }

        [Fact]
        public async Task Handle_ValidRequest_ReturnsPaginatedResult()
        {
            // Arrange
            var query = new GetUsersWithRolesPaginatedQuery { Page = 1, PageSize = 10 };
            var users = new List<User>
            {
                new User(
                    new UserName("testuser"),
                    new Email("test@test.com"),
                    new PasswordHash("12345678901234567890")
                ),
            };
            var userDtos = new List<UserWithRolesDto>
            {
                new UserWithRolesDto { Email = "test@test.com", UserName = "testuser" },
            };

            _userServiceMock
                .Setup(x => x.GetUsersWithRolesPaginatedAsync(1, 10, It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result<(List<User> Users, int TotalCount)>.Success((users, 1)));

            _mapperMock.Setup(x => x.Map<List<UserWithRolesDto>>(users)).Returns(userDtos);

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
            var query = new GetUsersWithRolesPaginatedQuery { Page = 1, PageSize = 10 };
            _userServiceMock
                .Setup(x => x.GetUsersWithRolesPaginatedAsync(1, 10, It.IsAny<CancellationToken>()))
                .ReturnsAsync(
                    Result<(List<User> Users, int TotalCount)>.Failure(
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

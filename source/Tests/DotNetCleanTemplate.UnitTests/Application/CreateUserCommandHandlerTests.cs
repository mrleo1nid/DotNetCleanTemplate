using DotNetCleanTemplate.Application.Features.Auth.RegisterUser;
using DotNetCleanTemplate.Application.Features.Users.CreateUser;
using DotNetCleanTemplate.Shared.Common;
using DotNetCleanTemplate.Shared.DTOs;
using MediatR;
using Moq;
using Xunit;

namespace DotNetCleanTemplate.UnitTests.Application;

public class CreateUserCommandHandlerTests
{
    private readonly Mock<IMediator> _mediatorMock;
    private readonly CreateUserCommandHandler _handler;

    public CreateUserCommandHandlerTests()
    {
        _mediatorMock = new Mock<IMediator>();
        _handler = new CreateUserCommandHandler(_mediatorMock.Object);
    }

    [Fact]
    public async Task Handle_Should_Return_Success_When_Registration_Succeeds()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var createUserDto = new CreateUserDto
        {
            UserName = "testuser",
            Email = "test@example.com",
            Password = "Password123",
            ConfirmPassword = "Password123",
        };

        var command = new CreateUserCommand { Dto = createUserDto };
        var expectedResult = Result<Guid>.Success(userId);

        _mediatorMock
            .Setup(x => x.Send(It.IsAny<RegisterUserCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResult);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(userId, result.Value);
        _mediatorMock.Verify(
            x =>
                x.Send(
                    It.Is<RegisterUserCommand>(cmd =>
                        cmd.Dto.UserName == createUserDto.UserName
                        && cmd.Dto.Email == createUserDto.Email
                        && cmd.Dto.Password == createUserDto.Password
                    ),
                    It.IsAny<CancellationToken>()
                ),
            Times.Once
        );
    }

    [Fact]
    public async Task Handle_Should_Return_Failure_When_Registration_Fails()
    {
        // Arrange
        var createUserDto = new CreateUserDto
        {
            UserName = "testuser",
            Email = "test@example.com",
            Password = "Password123",
            ConfirmPassword = "Password123",
        };

        var command = new CreateUserCommand { Dto = createUserDto };
        var expectedResult = Result<Guid>.Failure("User.AlreadyExists", "User already exists");

        _mediatorMock
            .Setup(x => x.Send(It.IsAny<RegisterUserCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResult);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal("User already exists", result.Errors.First().Message);
    }

    [Fact]
    public async Task Handle_Should_Transform_Dto_Correctly()
    {
        // Arrange
        var createUserDto = new CreateUserDto
        {
            UserName = "testuser",
            Email = "test@example.com",
            Password = "Password123",
            ConfirmPassword = "Password456", // Это поле не должно передаваться в RegisterUserCommand
        };

        var command = new CreateUserCommand { Dto = createUserDto };
        var expectedResult = Result<Guid>.Success(Guid.NewGuid());

        RegisterUserCommand capturedCommand = null!;
        _mediatorMock
            .Setup(x => x.Send(It.IsAny<RegisterUserCommand>(), It.IsAny<CancellationToken>()))
            .Callback<IRequest<Result<Guid>>, CancellationToken>(
                (cmd, _) => capturedCommand = (RegisterUserCommand)cmd
            )
            .ReturnsAsync(expectedResult);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotNull(capturedCommand);
        Assert.Equal(createUserDto.UserName, capturedCommand.Dto.UserName);
        Assert.Equal(createUserDto.Email, capturedCommand.Dto.Email);
        Assert.Equal(createUserDto.Password, capturedCommand.Dto.Password);
        // ConfirmPassword не должно передаваться в RegisterUserCommand
    }
}

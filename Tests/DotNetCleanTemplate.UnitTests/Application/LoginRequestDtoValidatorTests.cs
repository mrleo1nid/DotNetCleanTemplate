using DotNetCleanTemplate.Application.Validation;
using DotNetCleanTemplate.Shared.DTOs;
using DotNetCleanTemplate.UnitTests.Common;

namespace DotNetCleanTemplate.UnitTests.Application
{
    public class LoginRequestDtoValidatorTests
        : ValidatorTestBase<LoginRequestDtoValidator, LoginRequestDto>
    {
        [Fact]
        public void Should_Pass_For_Valid_Dto()
        {
            var dto = new LoginRequestDto
            {
                Email = CreateValidEmail(),
                Password = CreateValidPassword(),
            };
            ShouldPass(dto);
        }

        [Theory]
        [InlineData("")]
        [InlineData("bademail")]
        public void Should_Fail_For_Invalid_Email(string email)
        {
            var dto = new LoginRequestDto { Email = email, Password = CreateValidPassword() };
            ShouldFail(dto, nameof(LoginRequestDto.Email));
        }

        [Fact]
        public void Should_Fail_For_Null_Email()
        {
            var dto = new LoginRequestDto { Email = null!, Password = CreateValidPassword() };
            ShouldFail(dto, nameof(LoginRequestDto.Email));
        }

        [Theory]
        [InlineData("")]
        [InlineData("123")]
        public void Should_Fail_For_Invalid_Password(string password)
        {
            var dto = new LoginRequestDto { Email = CreateValidEmail(), Password = password };
            ShouldFail(dto, nameof(LoginRequestDto.Password));
        }

        [Fact]
        public void Should_Fail_For_Null_Password()
        {
            var dto = new LoginRequestDto { Email = CreateValidEmail(), Password = null! };
            ShouldFail(dto, nameof(LoginRequestDto.Password));
        }
    }
}

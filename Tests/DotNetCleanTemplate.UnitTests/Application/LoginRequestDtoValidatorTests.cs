using DotNetCleanTemplate.Application.Validation;
using DotNetCleanTemplate.Shared.DTOs;
using FluentValidation.TestHelper;

namespace DotNetCleanTemplate.UnitTests.Application
{
    public class LoginRequestDtoValidatorTests
    {
        private readonly LoginRequestDtoValidator _validator = new();

        [Fact]
        public void Should_Pass_For_Valid_Dto()
        {
            var dto = new LoginRequestDto
            {
                Email = "user@example.com",
                Password = new string('a', 10),
            };
            var result = _validator.TestValidate(dto);
            result.ShouldNotHaveAnyValidationErrors();
        }

        [Theory]
        [InlineData("")]
        [InlineData("bademail")]
        public void Should_Fail_For_Invalid_Email(string email)
        {
            var dto = new LoginRequestDto { Email = email, Password = new string('a', 10) };
            var result = _validator.TestValidate(dto);
            result.ShouldHaveValidationErrorFor(x => x.Email);
        }

        [Fact]
        public void Should_Fail_For_Null_Email()
        {
            var dto = new LoginRequestDto { Email = null!, Password = new string('a', 10) };
            var result = _validator.TestValidate(dto);
            result.ShouldHaveValidationErrorFor(x => x.Email);
        }

        [Theory]
        [InlineData("")]
        [InlineData("123")]
        public void Should_Fail_For_Invalid_Password(string password)
        {
            var dto = new LoginRequestDto { Email = "user@example.com", Password = password };
            var result = _validator.TestValidate(dto);
            result.ShouldHaveValidationErrorFor(x => x.Password);
        }

        [Fact]
        public void Should_Fail_For_Null_Password()
        {
            var dto = new LoginRequestDto { Email = "user@example.com", Password = null! };
            var result = _validator.TestValidate(dto);
            result.ShouldHaveValidationErrorFor(x => x.Password);
        }
    }
}

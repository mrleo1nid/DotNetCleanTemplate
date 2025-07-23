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
                Email = CreateValidEmail(),
                Password = CreateValidPassword(),
            };
            var result = _validator.TestValidate(dto);
            result.ShouldNotHaveAnyValidationErrors();
        }

        [Theory]
        [InlineData("")]
        [InlineData("bademail")]
        public void Should_Fail_For_Invalid_Email(string email)
        {
            var dto = new LoginRequestDto { Email = email, Password = CreateValidPassword() };
            var result = _validator.TestValidate(dto);
            result.ShouldHaveValidationErrorFor(x => x.Email);
        }

        [Fact]
        public void Should_Fail_For_Null_Email()
        {
            var dto = new LoginRequestDto { Email = null!, Password = CreateValidPassword() };
            var result = _validator.TestValidate(dto);
            result.ShouldHaveValidationErrorFor(x => x.Email);
        }

        [Theory]
        [InlineData("")]
        [InlineData("123")]
        public void Should_Fail_For_Invalid_Password(string password)
        {
            var dto = new LoginRequestDto { Email = CreateValidEmail(), Password = password };
            var result = _validator.TestValidate(dto);
            result.ShouldHaveValidationErrorFor(x => x.Password);
        }

        [Fact]
        public void Should_Fail_For_Null_Password()
        {
            var dto = new LoginRequestDto { Email = CreateValidEmail(), Password = null! };
            var result = _validator.TestValidate(dto);
            result.ShouldHaveValidationErrorFor(x => x.Password);
        }

        private static string CreateValidEmail()
        {
            return $"test{Guid.NewGuid()}@example.com";
        }

        private static string CreateValidPassword()
        {
            return "12345678901234567890";
        }
    }
}

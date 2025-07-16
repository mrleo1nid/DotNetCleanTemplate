using DotNetCleanTemplate.Application.Validation;
using DotNetCleanTemplate.Shared.DTOs;
using FluentValidation.TestHelper;
using Xunit;

namespace ApplicationTests
{
    public class RegisterUserDtoValidatorTests
    {
        private readonly RegisterUserDtoValidator _validator = new();

        [Fact]
        public void Should_Pass_For_Valid_Dto()
        {
            var dto = new RegisterUserDto
            {
                UserName = "ValidUser",
                Email = "user@example.com",
                Password = new string('a', 10),
            };
            var result = _validator.TestValidate(dto);
            result.ShouldNotHaveAnyValidationErrors();
        }

        [Theory]
        [InlineData("")]
        [InlineData("ab")]
        public void Should_Fail_For_Invalid_UserName(string userName)
        {
            var dto = new RegisterUserDto
            {
                UserName = userName,
                Email = "user@example.com",
                Password = new string('a', 10),
            };
            var result = _validator.TestValidate(dto);
            result.ShouldHaveValidationErrorFor(x => x.UserName);
        }

        [Fact]
        public void Should_Fail_For_Null_UserName()
        {
            var dto = new RegisterUserDto
            {
                UserName = null!,
                Email = "user@example.com",
                Password = new string('a', 10),
            };
            var result = _validator.TestValidate(dto);
            result.ShouldHaveValidationErrorFor(x => x.UserName);
        }

        [Theory]
        [InlineData("")]
        [InlineData("bademail")]
        public void Should_Fail_For_Invalid_Email(string email)
        {
            var dto = new RegisterUserDto
            {
                UserName = "ValidUser",
                Email = email,
                Password = new string('a', 10),
            };
            var result = _validator.TestValidate(dto);
            result.ShouldHaveValidationErrorFor(x => x.Email);
        }

        [Fact]
        public void Should_Fail_For_Null_Email()
        {
            var dto = new RegisterUserDto
            {
                UserName = "ValidUser",
                Email = null!,
                Password = new string('a', 10),
            };
            var result = _validator.TestValidate(dto);
            result.ShouldHaveValidationErrorFor(x => x.Email);
        }

        [Theory]
        [InlineData("")]
        [InlineData("123")]
        public void Should_Fail_For_Invalid_Password(string password)
        {
            var dto = new RegisterUserDto
            {
                UserName = "ValidUser",
                Email = "user@example.com",
                Password = password,
            };
            var result = _validator.TestValidate(dto);
            result.ShouldHaveValidationErrorFor(x => x.Password);
        }

        [Fact]
        public void Should_Fail_For_Null_Password()
        {
            var dto = new RegisterUserDto
            {
                UserName = "ValidUser",
                Email = "user@example.com",
                Password = null!,
            };
            var result = _validator.TestValidate(dto);
            result.ShouldHaveValidationErrorFor(x => x.Password);
        }
    }
}

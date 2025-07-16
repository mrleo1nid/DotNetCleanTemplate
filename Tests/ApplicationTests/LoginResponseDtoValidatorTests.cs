using DotNetCleanTemplate.Application.Validation;
using DotNetCleanTemplate.Shared.DTOs;
using FluentValidation.TestHelper;
using Xunit;

namespace ApplicationTests
{
    public class LoginResponseDtoValidatorTests
    {
        private readonly LoginResponseDtoValidator _validator = new();

        [Fact]
        public void Should_Pass_For_Valid_Dto()
        {
            var dto = new LoginResponseDto
            {
                AccessToken = "token",
                RefreshToken = "refresh",
                RefreshTokenExpires = System.DateTime.UtcNow.AddDays(1),
            };
            var result = _validator.TestValidate(dto);
            result.ShouldNotHaveAnyValidationErrors();
        }

        [Theory]
        [InlineData("")]
        public void Should_Fail_For_Invalid_AccessToken(string token)
        {
            var dto = new LoginResponseDto
            {
                AccessToken = token,
                RefreshToken = "refresh",
                RefreshTokenExpires = System.DateTime.UtcNow.AddDays(1),
            };
            var result = _validator.TestValidate(dto);
            result.ShouldHaveValidationErrorFor(x => x.AccessToken);
        }

        [Fact]
        public void Should_Fail_For_Null_AccessToken()
        {
            var dto = new LoginResponseDto
            {
                AccessToken = null!,
                RefreshToken = "refresh",
                RefreshTokenExpires = System.DateTime.UtcNow.AddDays(1),
            };
            var result = _validator.TestValidate(dto);
            result.ShouldHaveValidationErrorFor(x => x.AccessToken);
        }

        [Theory]
        [InlineData("")]
        public void Should_Fail_For_Invalid_RefreshToken(string token)
        {
            var dto = new LoginResponseDto
            {
                AccessToken = "token",
                RefreshToken = token,
                RefreshTokenExpires = System.DateTime.UtcNow.AddDays(1),
            };
            var result = _validator.TestValidate(dto);
            result.ShouldHaveValidationErrorFor(x => x.RefreshToken);
        }

        [Fact]
        public void Should_Fail_For_Null_RefreshToken()
        {
            var dto = new LoginResponseDto
            {
                AccessToken = "token",
                RefreshToken = null!,
                RefreshTokenExpires = System.DateTime.UtcNow.AddDays(1),
            };
            var result = _validator.TestValidate(dto);
            result.ShouldHaveValidationErrorFor(x => x.RefreshToken);
        }
    }
}

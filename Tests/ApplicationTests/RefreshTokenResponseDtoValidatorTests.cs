using DotNetCleanTemplate.Application.Validation;
using DotNetCleanTemplate.Shared.DTOs;
using FluentValidation.TestHelper;

namespace ApplicationTests
{
    public class RefreshTokenResponseDtoValidatorTests
    {
        private readonly RefreshTokenResponseDtoValidator _validator = new();

        [Fact]
        public void Should_Pass_For_Valid_Dto()
        {
            var dto = new RefreshTokenResponseDto
            {
                AccessToken = "token",
                RefreshToken = "refresh",
                Expires = System.DateTime.UtcNow.AddDays(1),
            };
            var result = _validator.TestValidate(dto);
            result.ShouldNotHaveAnyValidationErrors();
        }

        [Theory]
        [InlineData("")]
        public void Should_Fail_For_Invalid_AccessToken(string token)
        {
            var dto = new RefreshTokenResponseDto
            {
                AccessToken = token,
                RefreshToken = "refresh",
                Expires = System.DateTime.UtcNow.AddDays(1),
            };
            var result = _validator.TestValidate(dto);
            result.ShouldHaveValidationErrorFor(x => x.AccessToken);
        }

        [Fact]
        public void Should_Fail_For_Null_AccessToken()
        {
            var dto = new RefreshTokenResponseDto
            {
                AccessToken = null!,
                RefreshToken = "refresh",
                Expires = System.DateTime.UtcNow.AddDays(1),
            };
            var result = _validator.TestValidate(dto);
            result.ShouldHaveValidationErrorFor(x => x.AccessToken);
        }

        [Theory]
        [InlineData("")]
        public void Should_Fail_For_Invalid_RefreshToken(string token)
        {
            var dto = new RefreshTokenResponseDto
            {
                AccessToken = "token",
                RefreshToken = token,
                Expires = System.DateTime.UtcNow.AddDays(1),
            };
            var result = _validator.TestValidate(dto);
            result.ShouldHaveValidationErrorFor(x => x.RefreshToken);
        }

        [Fact]
        public void Should_Fail_For_Null_RefreshToken()
        {
            var dto = new RefreshTokenResponseDto
            {
                AccessToken = "token",
                RefreshToken = null!,
                Expires = System.DateTime.UtcNow.AddDays(1),
            };
            var result = _validator.TestValidate(dto);
            result.ShouldHaveValidationErrorFor(x => x.RefreshToken);
        }
    }
}

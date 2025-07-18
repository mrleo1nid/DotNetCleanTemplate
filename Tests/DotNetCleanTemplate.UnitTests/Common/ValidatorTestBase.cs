using FluentValidation;
using FluentValidation.TestHelper;

namespace DotNetCleanTemplate.UnitTests.Common
{
    public abstract class ValidatorTestBase<TValidator, TModel>
        where TValidator : class, IValidator<TModel>, new()
        where TModel : class
    {
        protected readonly TValidator Validator = new();

        protected TestValidationResult<TModel> Validate(TModel model)
        {
            return Validator.TestValidate(model);
        }

        protected void ShouldPass(TModel model)
        {
            var result = Validate(model);
            result.ShouldNotHaveAnyValidationErrors();
        }

        protected void ShouldFail(TModel model, string propertyName)
        {
            var result = Validate(model);
            result.ShouldHaveValidationErrorFor(propertyName);
        }

        protected void ShouldFail(TModel model, string propertyName, string errorMessage)
        {
            var result = Validate(model);
            result.ShouldHaveValidationErrorFor(propertyName).WithErrorMessage(errorMessage);
        }

        protected static string CreateValidEmail()
        {
            return $"test{Guid.NewGuid()}@example.com";
        }

        protected static string CreateInvalidEmail()
        {
            return "invalid-email";
        }

        protected static string CreateValidPassword()
        {
            return "12345678901234567890";
        }

        protected static string CreateInvalidPassword()
        {
            return "123";
        }

        protected static string CreateValidUserName()
        {
            return "TestUser";
        }

        protected static string CreateInvalidUserName()
        {
            return "ab"; // слишком короткое
        }
    }
}

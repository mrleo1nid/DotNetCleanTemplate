using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;

namespace IntegrationTests
{
    public class RegisterUserEndpointTests : TestBase
    {
        [Fact]
        public async Task RegisterUser_WithValidData_ReturnsUserId()
        {
            var unique = Guid.NewGuid().ToString("N").Substring(0, 8);
            var registerRequest = new
            {
                userName = $"testuser_{unique}",
                email = $"test_{unique}@example.com",
                password = "Test123!",
            };

            var response = await Client!.PostAsJsonAsync("/auth/register", registerRequest);
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var content = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(content);
            doc.RootElement.TryGetProperty("isSuccess", out var isSuccessProp).Should().BeTrue();
            isSuccessProp.GetBoolean().Should().BeTrue();
            doc.RootElement.TryGetProperty("value", out var valueProp).Should().BeTrue();
            Guid.TryParse(valueProp.GetString(), out var guid).Should().BeTrue();
            guid.Should().NotBe(Guid.Empty);
        }
    }
}

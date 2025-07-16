using System.Net;
using System.Text.Json;
using DotNetCleanTemplate.Api;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Xunit.Abstractions;

namespace IntegrationTests
{
    public class GetAllUsersWithRolesEndpointTests : TestBase
    {
        public GetAllUsersWithRolesEndpointTests(CustomWebApplicationFactory<Program> factory)
            : base(factory) { }

        [Fact]
        public async Task GetAllUsersWithRoles_ReturnsList()
        {
            Console.WriteLine("[TestBase] Configuration:");
            using (var scope = Factory.Services.CreateScope())
            {
                var config = scope.ServiceProvider.GetRequiredService<IConfiguration>();
                PrintConfiguration(config);
            }

            // Act
            var response = await Client!.GetAsync("/users");
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var content = await response.Content.ReadAsStringAsync();
            // Проверяем, что это Result<List<UserWithRolesDto>>
            using var doc = JsonDocument.Parse(content);
            doc.RootElement.TryGetProperty("isSuccess", out var isSuccessProp).Should().BeTrue();
            isSuccessProp.GetBoolean().Should().BeTrue();
            doc.RootElement.TryGetProperty("value", out var valueProp).Should().BeTrue();
            valueProp.ValueKind.Should().Be(JsonValueKind.Array);
        }
    }
}

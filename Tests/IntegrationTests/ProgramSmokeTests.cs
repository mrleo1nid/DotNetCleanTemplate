using DotNetCleanTemplate.Api;
using FluentAssertions;
using System.Net;

namespace IntegrationTests
{
    public class ProgramSmokeTests : TestBase
    {
        public ProgramSmokeTests(CustomWebApplicationFactory<Program> factory)
            : base(factory) { }

        [Fact]
        public async Task Application_Starts_And_RespondsToHello()
        {
            var response = await Client!.GetAsync("tests/hello");
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var content = await response.Content.ReadAsStringAsync();
            content.Should().Contain("Hello");
        }
    }
}

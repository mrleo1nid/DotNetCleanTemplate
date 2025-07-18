using System.Net;
using DotNetCleanTemplate.Api;
using DotNetCleanTemplate.IntegrationTests.Common;
using FluentAssertions;

namespace DotNetCleanTemplate.IntegrationTests.Endpoints
{
    public class CorsExtensionTests : TestBase
    {
        public CorsExtensionTests(CustomWebApplicationFactory<Program> factory)
            : base(factory) { }

        [Fact]
        public async Task CorsPolicy_AllowsConfiguredOrigin()
        {
            var request = new HttpRequestMessage(HttpMethod.Options, "/hello");
            request.Headers.Add("Origin", "http://localhost:3000");
            request.Headers.Add("Access-Control-Request-Method", "GET");

            var response = await Client!.SendAsync(request);
            response
                .StatusCode.Should()
                .Match(x => x == HttpStatusCode.NoContent || x == HttpStatusCode.OK);
            response.Headers.Contains("Access-Control-Allow-Origin").Should().BeTrue();
            response
                .Headers.GetValues("Access-Control-Allow-Origin")
                .Should()
                .Contain("http://localhost:3000");
        }
    }
}

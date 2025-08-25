using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace Villainous.Api.Tests;

public class HealthCheckTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient client;

    public HealthCheckTests(WebApplicationFactory<Program> factory)
    {
        client = factory.CreateClient();
    }

    [Theory]
    [InlineData("/healthz/live")]
    [InlineData("/ready")]
    public async Task ReturnsOk(string url)
    {
        var response = await client.GetAsync(url);
        response.EnsureSuccessStatusCode();
    }
}

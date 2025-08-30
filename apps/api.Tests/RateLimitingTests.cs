using System.Net;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace Villainous.Api.Tests;

public class RateLimitingTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient client;

    public RateLimitingTests(WebApplicationFactory<Program> factory)
    {
        client = factory.CreateClient();
    }

    [Fact]
    public async Task Returns429WhenLimitExceeded()
    {
        for (var i = 0; i < 5; i++)
        {
            var ok = await client.GetAsync("/ready");
            Assert.True(ok.IsSuccessStatusCode);
        }

        var rejected = await client.GetAsync("/ready");
        Assert.Equal(HttpStatusCode.TooManyRequests, rejected.StatusCode);
    }
}


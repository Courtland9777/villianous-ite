using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Villainous.Model;
using Xunit;

namespace Villainous.Api.Tests;

public class ApiVersioningTests : IClassFixture<TestingWebApplicationFactory>
{
    private readonly HttpClient client;

    public ApiVersioningTests(TestingWebApplicationFactory factory)
    {
        client = factory.CreateClient();
    }

    [Fact]
    public async Task AcceptsSupportedApiVersion()
    {
        var request = new HttpRequestMessage(HttpMethod.Post, "/api/matches")
        {
            Content = JsonContent.Create(new CreateMatchRequest(["Prince John", "Captain Hook"]))
        };
        request.Headers.Add("api-version", "1.0");
        var response = await client.SendAsync(request);
        response.EnsureSuccessStatusCode();
    }

    [Fact]
    public async Task RejectsUnsupportedApiVersion()
    {
        var request = new HttpRequestMessage(HttpMethod.Post, "/api/matches")
        {
            Content = JsonContent.Create(new CreateMatchRequest(["Prince John", "Captain Hook"]))
        };
        request.Headers.Add("api-version", "2.0");
        var response = await client.SendAsync(request);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}

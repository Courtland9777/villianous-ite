using System;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Villainous.Model;
using Xunit;

namespace Villainous.Api.Tests;

public class CreateMatchTests : IClassFixture<TestingWebApplicationFactory>
{
    private readonly HttpClient client;

    public CreateMatchTests(TestingWebApplicationFactory factory)
    {
        client = factory.CreateClient();
    }

    [Fact]
    public async Task ReturnsMatchId()
    {
        var request = new CreateMatchRequest(["Prince John", "Captain Hook"]);
        var response = await client.PostAsJsonAsync("/api/matches", request);
        response.EnsureSuccessStatusCode();
        var body = await response.Content.ReadFromJsonAsync<CreateMatchResponse>();
        Assert.NotNull(body);
        Assert.NotEqual(Guid.Empty, body!.MatchId);
    }
}

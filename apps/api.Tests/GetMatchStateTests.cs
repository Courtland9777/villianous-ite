using System;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Villainous.Engine;
using Villainous.Model;
using Xunit;

namespace Villainous.Api.Tests;

public class GetMatchStateTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient client;

    public GetMatchStateTests(WebApplicationFactory<Program> factory)
    {
        client = factory.CreateClient();
    }

    [Fact]
    public async Task ReturnsState()
    {
        var request = new CreateMatchRequest(["Prince John", "Captain Hook"]);
        var create = await client.PostAsJsonAsync("/api/matches", request);
        var match = await create.Content.ReadFromJsonAsync<CreateMatchResponse>();

        var state = await client.GetFromJsonAsync<GameState>($"/api/matches/{match!.MatchId}/state");

        Assert.NotNull(state);
        Assert.Equal(match!.MatchId, state!.MatchId);
        Assert.Equal(2, state.Players.Count);
    }

    [Fact]
    public async Task Returns404ForUnknownId()
    {
        var response = await client.GetAsync($"/api/matches/{Guid.NewGuid()}/state");
        Assert.Equal(System.Net.HttpStatusCode.NotFound, response.StatusCode);
    }
}

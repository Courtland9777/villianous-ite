using System;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Villainous.Engine;
using Villainous.Model;
using Xunit;

namespace Villainous.Api.Tests;

public class PostMatchCommandsTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient client;

    public PostMatchCommandsTests(WebApplicationFactory<Program> factory)
    {
        client = factory.CreateClient();
    }

    [Fact]
    public async Task PersistsEventsToReplay()
    {
        var create = await client.PostAsJsonAsync("/api/matches", new CreateMatchRequest(["Prince John", "Captain Hook"]));
        var match = await create.Content.ReadFromJsonAsync<CreateMatchResponse>();
        var state = await client.GetFromJsonAsync<GameState>($"/api/matches/{match!.MatchId}/state");
        var player = state!.Players[0].Id;
        var target = state.Players[1].Id;

        var command = new SubmitCommandRequest("Fate", player, target, null, null, "Ariel");
        var response = await client.PostAsJsonAsync($"/api/matches/{match!.MatchId}/commands", command);
        response.EnsureSuccessStatusCode();

        var replay = await client.GetFromJsonAsync<JsonElement[]>($"/api/matches/{match.MatchId}/replay");
        Assert.Single(replay!);
    }

    [Fact]
    public async Task Returns404ForUnknownId()
    {
        var command = new SubmitCommandRequest("CheckObjective", Guid.NewGuid(), null, null, null, null);
        var response = await client.PostAsJsonAsync($"/api/matches/{Guid.NewGuid()}/commands", command);
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Returns400ForUnknownType()
    {
        var create = await client.PostAsJsonAsync("/api/matches", new CreateMatchRequest(["Prince John", "Captain Hook"]));
        var match = await create.Content.ReadFromJsonAsync<CreateMatchResponse>();
        var command = new SubmitCommandRequest("Unknown", Guid.NewGuid(), null, null, null, null);
        var response = await client.PostAsJsonAsync($"/api/matches/{match!.MatchId}/commands", command);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}

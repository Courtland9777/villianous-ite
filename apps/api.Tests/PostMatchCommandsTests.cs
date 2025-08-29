using System;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Testing;
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
        var state = await client.GetFromJsonAsync<GameStateDto>($"/api/matches/{match!.MatchId}/state");
        var player = state!.Players[0].Id;
        var target = state.Players[1].Id;

        var command = new SubmitCommandRequest("Fate", player, 1, target, null, null, "Ariel");
        var response = await client.PostAsJsonAsync($"/api/matches/{match!.MatchId}/commands", command);
        response.EnsureSuccessStatusCode();

        var replay = await client.GetFromJsonAsync<JsonElement[]>($"/api/matches/{match.MatchId}/replay");
        Assert.Single(replay!);
    }

    [Fact]
    public async Task ExecutesCheckObjectiveCommand()
    {
        var create = await client.PostAsJsonAsync("/api/matches", new CreateMatchRequest(["Prince John", "Captain Hook"]));
        var match = await create.Content.ReadFromJsonAsync<CreateMatchResponse>();
        var state = await client.GetFromJsonAsync<GameStateDto>($"/api/matches/{match!.MatchId}/state");
        var player = state!.Players[0].Id;

        var command = new SubmitCommandRequest("CheckObjective", player, 1, null, null, null, null);
        var response = await client.PostAsJsonAsync($"/api/matches/{match.MatchId}/commands", command);
        response.EnsureSuccessStatusCode();
    }

    [Fact]
    public async Task AcceptsVanquishCommand()
    {
        var create = await client.PostAsJsonAsync("/api/matches", new CreateMatchRequest(["Prince John", "Captain Hook"]));
        var match = await create.Content.ReadFromJsonAsync<CreateMatchResponse>();
        var state = await client.GetFromJsonAsync<GameStateDto>($"/api/matches/{match!.MatchId}/state");
        var player = state!.Players[0].Id;

        var command = new SubmitCommandRequest("Vanquish", player, 1, null, "Realm", "Hero", null);
        var response = await client.PostAsJsonAsync($"/api/matches/{match.MatchId}/commands", command);
        response.EnsureSuccessStatusCode();
    }

    [Fact]
    public async Task Returns404ForUnknownId()
    {
        var command = new SubmitCommandRequest("CheckObjective", Guid.NewGuid(), 1, null, null, null, null);
        var response = await client.PostAsJsonAsync($"/api/matches/{Guid.NewGuid()}/commands", command);
        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.Equal("Match not found", problem!.Title);
        Assert.Equal("match.not_found", problem.Extensions["code"]?.ToString());
        Assert.False(string.IsNullOrEmpty(problem.Extensions["traceId"]?.ToString()));
    }

    [Fact]
    public async Task Returns400ForUnknownType()
    {
        var create = await client.PostAsJsonAsync("/api/matches", new CreateMatchRequest(["Prince John", "Captain Hook"]));
        var match = await create.Content.ReadFromJsonAsync<CreateMatchResponse>();
        var command = new SubmitCommandRequest("Unknown", Guid.NewGuid(), 1, null, null, null, null);
        var response = await client.PostAsJsonAsync($"/api/matches/{match!.MatchId}/commands", command);
        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("Unknown command type", problem!.Title);
        Assert.Equal("command.unknown_type", problem.Extensions["code"]?.ToString());
        Assert.False(string.IsNullOrEmpty(problem.Extensions["traceId"]?.ToString()));
    }

    [Fact]
    public async Task Returns409ForDuplicateClientSeq()
    {
        var create = await client.PostAsJsonAsync("/api/matches", new CreateMatchRequest(["Prince John", "Captain Hook"]));
        var match = await create.Content.ReadFromJsonAsync<CreateMatchResponse>();
        var state = await client.GetFromJsonAsync<GameStateDto>($"/api/matches/{match!.MatchId}/state");
        var player = state!.Players[0].Id;
        var target = state.Players[1].Id;

        var command = new SubmitCommandRequest("Fate", player, 1, target, null, null, "Ariel");
        await client.PostAsJsonAsync($"/api/matches/{match.MatchId}/commands", command);

        var second = await client.PostAsJsonAsync($"/api/matches/{match.MatchId}/commands", command);
        var problem = await second.Content.ReadFromJsonAsync<ProblemDetails>();
        Assert.Equal(HttpStatusCode.Conflict, second.StatusCode);
        Assert.Equal("command.duplicate", problem!.Extensions["code"]?.ToString());
        Assert.False(string.IsNullOrEmpty(problem.Extensions["traceId"]?.ToString()));
    }
}

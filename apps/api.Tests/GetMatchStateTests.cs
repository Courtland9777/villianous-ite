using System;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Testing;
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

        var state = await client.GetFromJsonAsync<GameStateDto>($"/api/matches/{match!.MatchId}/state");

        Assert.NotNull(state);
        Assert.Equal(match!.MatchId, state!.MatchId);
        Assert.Equal(2, state.Players.Count);
    }

    [Fact]
    public async Task Returns404ForUnknownId()
    {
        var response = await client.GetAsync($"/api/matches/{Guid.NewGuid()}/state");
        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        Assert.Equal(System.Net.HttpStatusCode.NotFound, response.StatusCode);
        Assert.Equal("Match not found", problem!.Title);
        Assert.Equal("match.not_found", problem.Extensions["code"]?.ToString());
        Assert.False(string.IsNullOrEmpty(problem.Extensions["traceId"]?.ToString()));
    }
}

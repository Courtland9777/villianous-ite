using System;
using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Testing;
using Villainous.Model;
using Xunit;

namespace Villainous.Api.Tests;

public class InputValidationTests : IClassFixture<TestingWebApplicationFactory>
{
    private readonly HttpClient client;

    public InputValidationTests(TestingWebApplicationFactory factory)
    {
        client = factory.CreateClient();
    }

    [Fact]
    public async Task CreateMatchRejectsEmptyVillain()
    {
        var response = await client.PostAsJsonAsync("/api/matches", new CreateMatchRequest(["", "Captain Hook"]));
        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("match.invalid_villains", problem!.Extensions["code"]?.ToString());
    }

    [Fact]
    public async Task SubmitCommandTrimsStrings()
    {
        var create = await client.PostAsJsonAsync("/api/matches", new CreateMatchRequest(["Prince John", "Captain Hook"]));
        var match = await create.Content.ReadFromJsonAsync<CreateMatchResponse>();
        var state = await client.GetFromJsonAsync<GameStateDto>($"/api/matches/{match!.MatchId}/state");
        var player = state!.Players[0].Id;

        var command = new SubmitCommandRequest(" Vanquish ", player, 1, null, " Realm ", " Hero ", null);
        var response = await client.PostAsJsonAsync($"/api/matches/{match.MatchId}/commands", command);
        response.EnsureSuccessStatusCode();
    }

    [Fact]
    public async Task SubmitCommandRejectsEmptyType()
    {
        var create = await client.PostAsJsonAsync("/api/matches", new CreateMatchRequest(["Prince John", "Captain Hook"]));
        var match = await create.Content.ReadFromJsonAsync<CreateMatchResponse>();
        var state = await client.GetFromJsonAsync<GameStateDto>($"/api/matches/{match!.MatchId}/state");
        var player = state!.Players[0].Id;

        var command = new SubmitCommandRequest("", player, 1, null, null, null, null);
        var response = await client.PostAsJsonAsync($"/api/matches/{match.MatchId}/commands", command);
        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("command.invalid_type", problem!.Extensions["code"]?.ToString());
    }
}


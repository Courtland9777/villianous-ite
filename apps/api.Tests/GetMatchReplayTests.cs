using System;
using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Testing;
using Villainous.Engine;
using Villainous.Model;
using Xunit;

namespace Villainous.Api.Tests;

public class GetMatchReplayTests : IClassFixture<TestingWebApplicationFactory>
{
    private readonly HttpClient client;

    public GetMatchReplayTests(TestingWebApplicationFactory factory)
    {
        client = factory.CreateClient();
    }

    [Fact]
    public async Task ReturnsEmptyReplay()
    {
        var request = new CreateMatchRequest(["Prince John", "Captain Hook"]);
        var create = await client.PostAsJsonAsync("/api/matches", request);
        var match = await create.Content.ReadFromJsonAsync<CreateMatchResponse>();

        var replay = await client.GetFromJsonAsync<DomainEvent[]>(
            $"/api/matches/{match!.MatchId}/replay");

        Assert.NotNull(replay);
        Assert.Empty(replay!);
    }

    [Fact]
    public async Task Returns404ForUnknownId()
    {
        var response = await client.GetAsync($"/api/matches/{Guid.NewGuid()}/replay");
        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.Equal("Match not found", problem!.Title);
        Assert.Equal("match.not_found", problem.Extensions["code"]?.ToString());
        Assert.False(string.IsNullOrEmpty(problem.Extensions["traceId"]?.ToString()));
    }
}

using System;
using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Villainous.Engine;
using Villainous.Model;
using Xunit;

namespace Villainous.Api.Tests;

public class GetMatchReplayTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient client;

    public GetMatchReplayTests(WebApplicationFactory<Program> factory)
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
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}

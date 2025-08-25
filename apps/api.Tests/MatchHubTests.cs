using System;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.SignalR.Client;
using Villainous.Engine;
using Villainous.Model;
using Xunit;

namespace Villainous.Api.Tests;

public class MatchHubTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> factory;

    public MatchHubTests(WebApplicationFactory<Program> factory)
    {
        this.factory = factory;
    }

    [Fact]
    public async Task SendCommandBroadcastsState()
    {
        var client = factory.CreateClient();
        var create = await client.PostAsJsonAsync("/api/matches", new CreateMatchRequest(["Prince John", "Captain Hook"]));
        var matchId = (await create.Content.ReadFromJsonAsync<CreateMatchResponse>())!.MatchId;
        var state = await client.GetFromJsonAsync<GameState>($"/api/matches/{matchId}/state");
        var playerId = state!.Players[0].Id;

        await using var connection1 = BuildConnection();
        await using var connection2 = BuildConnection();
        await connection1.StartAsync();
        await connection2.StartAsync();
        await connection1.InvokeAsync("JoinMatch", matchId);

        var tcs = new TaskCompletionSource<GameState>();
        var count = 0;
        connection2.On<GameState>("State", s =>
        {
            count++;
            if (count == 2)
            {
                tcs.TrySetResult(s);
            }
        });
        await connection2.InvokeAsync("JoinMatch", matchId);

        var command = new SubmitCommandRequest("CheckObjective", playerId, null, null, null, null);
        await connection1.InvokeAsync("SendCommand", matchId, command);
        var updated = await tcs.Task.WaitAsync(TimeSpan.FromSeconds(1));
        Assert.Equal(matchId, updated.MatchId);
    }

    private HubConnection BuildConnection() => new HubConnectionBuilder()
        .WithUrl($"{factory.Server.BaseAddress}hub/match", options =>
        {
            options.HttpMessageHandlerFactory = _ => factory.Server.CreateHandler();
        })
        .Build();
}

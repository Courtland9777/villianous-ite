using System;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.SignalR.Client;
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
        var state = await client.GetFromJsonAsync<GameStateDto>($"/api/matches/{matchId}/state");
        var playerId = state!.Players[0].Id;

        await using var connection1 = BuildConnection();
        await using var connection2 = BuildConnection();
        await connection1.StartAsync();
        await connection2.StartAsync();
        await connection1.InvokeAsync("JoinMatch", matchId);

        var tcs = new TaskCompletionSource<GameStateDto>();
        var count = 0;
        connection2.On<GameStateDto>("State", s =>
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

    [Fact]
    public async Task SendsCommandRejectedForUnknownType()
    {
        var client = factory.CreateClient();
        var create = await client.PostAsJsonAsync("/api/matches", new CreateMatchRequest(["Prince John", "Captain Hook"]));
        var matchId = (await create.Content.ReadFromJsonAsync<CreateMatchResponse>())!.MatchId;
        var state = await client.GetFromJsonAsync<GameStateDto>($"/api/matches/{matchId}/state");
        var playerId = state!.Players[0].Id;

        await using var connection = BuildConnection();
        await connection.StartAsync();
        await connection.InvokeAsync("JoinMatch", matchId);

        var tcs = new TaskCompletionSource<ProblemDetails>();
        connection.On<ProblemDetails>("CommandRejected", p => tcs.TrySetResult(p));

        var command = new SubmitCommandRequest("Unknown", playerId, null, null, null, null);
        await connection.InvokeAsync("SendCommand", matchId, command);

        var problem = await tcs.Task.WaitAsync(TimeSpan.FromSeconds(1));
        Assert.Equal(StatusCodes.Status400BadRequest, problem.Status);
        Assert.Equal("Unknown command type", problem.Title);
        Assert.Equal("command.unknown_type", problem.Extensions["code"]?.ToString());
        Assert.False(string.IsNullOrEmpty(problem.Extensions["traceId"]?.ToString()));
    }

    [Fact]
    public async Task JoinMatchRejectsUnknownId()
    {
        await using var connection = BuildConnection();
        await connection.StartAsync();

        var tcs = new TaskCompletionSource<ProblemDetails>();
        connection.On<ProblemDetails>("CommandRejected", p => tcs.TrySetResult(p));

        await connection.InvokeAsync("JoinMatch", Guid.NewGuid());

        var problem = await tcs.Task.WaitAsync(TimeSpan.FromSeconds(1));
        Assert.Equal(StatusCodes.Status404NotFound, problem.Status);
        Assert.Equal("match.not_found", problem.Extensions["code"]?.ToString());
        Assert.False(string.IsNullOrEmpty(problem.Extensions["traceId"]?.ToString()));
    }

    [Fact]
    public async Task ReconnectsUsingQueryString()
    {
        var client = factory.CreateClient();
        var create = await client.PostAsJsonAsync("/api/matches", new CreateMatchRequest(["Prince John", "Captain Hook"]));
        var matchId = (await create.Content.ReadFromJsonAsync<CreateMatchResponse>())!.MatchId;

        async Task<GameStateDto> ConnectAsync()
        {
            await using var connection = BuildConnection(matchId);
            var tcs = new TaskCompletionSource<GameStateDto>();
            connection.On<GameStateDto>("State", s => tcs.TrySetResult(s));
            await connection.StartAsync();
            return await tcs.Task.WaitAsync(TimeSpan.FromSeconds(1));
        }

        var first = await ConnectAsync();
        Assert.Equal(matchId, first.MatchId);

        var second = await ConnectAsync();
        Assert.Equal(matchId, second.MatchId);
    }

    private HubConnection BuildConnection(Guid? matchId = null) => new HubConnectionBuilder()
        .WithUrl(matchId is Guid id ? $"{factory.Server.BaseAddress}hub/match?matchId={id}" : $"{factory.Server.BaseAddress}hub/match", options =>
        {
            options.HttpMessageHandlerFactory = _ => factory.Server.CreateHandler();
        })
        .Build();
}

using System;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.SignalR.Client;
using Villainous.Api;
using Villainous.Model;
using Xunit;

namespace Villainous.Api.Tests;

public class MatchHubTests : IClassFixture<TestingWebApplicationFactory>
{
    private readonly TestingWebApplicationFactory factory;

    public MatchHubTests(TestingWebApplicationFactory factory)
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

        var tcs = new TaskCompletionSource<StateMessage>();
        var count = 0;
        connection2.On<StateMessage>("State", s =>
        {
            count++;
            if (count == 2)
            {
                tcs.TrySetResult(s);
            }
        });
        await connection2.InvokeAsync("JoinMatch", matchId);

        var command = new SubmitCommandRequest("CheckObjective", playerId, 1, null, null, null, null);
        await connection1.InvokeAsync("SendCommand", matchId, command);
        var updated = await tcs.Task.WaitAsync(TimeSpan.FromSeconds(1));
        Assert.Equal("1.0", updated.Version);
        Assert.Equal(matchId, updated.State.MatchId);
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

        var tcs = new TaskCompletionSource<ProblemMessage>();
        connection.On<ProblemMessage>("CommandRejected", p => tcs.TrySetResult(p));

        var command = new SubmitCommandRequest("Unknown", playerId, 1, null, null, null, null);
        await connection.InvokeAsync("SendCommand", matchId, command);

        var problem = await tcs.Task.WaitAsync(TimeSpan.FromSeconds(1));
        Assert.Equal("1.0", problem.Version);
        Assert.Equal(StatusCodes.Status400BadRequest, problem.Problem.Status);
        Assert.Equal("Unknown command type", problem.Problem.Title);
        Assert.Equal("command.unknown_type", problem.Problem.Extensions["code"]?.ToString());
        Assert.False(string.IsNullOrEmpty(problem.Problem.Extensions["traceId"]?.ToString()));
    }

    [Fact]
    public async Task SendCommandRejectsDuplicateClientSeq()
    {
        var client = factory.CreateClient();
        var create = await client.PostAsJsonAsync("/api/matches", new CreateMatchRequest(["Prince John", "Captain Hook"]));
        var matchId = (await create.Content.ReadFromJsonAsync<CreateMatchResponse>())!.MatchId;
        var state = await client.GetFromJsonAsync<GameStateDto>($"/api/matches/{matchId}/state");
        var playerId = state!.Players[0].Id;
        var targetId = state.Players[1].Id;

        await using var connection = BuildConnection();
        await connection.StartAsync();
        await connection.InvokeAsync("JoinMatch", matchId);

        var command = new SubmitCommandRequest("Fate", playerId, 1, targetId, null, null, "Ariel");
        await connection.InvokeAsync("SendCommand", matchId, command);

        var tcs = new TaskCompletionSource<ProblemMessage>();
        connection.On<ProblemMessage>("CommandRejected", p => tcs.TrySetResult(p));

        await connection.InvokeAsync("SendCommand", matchId, command);

        var problem = await tcs.Task.WaitAsync(TimeSpan.FromSeconds(1));
        Assert.Equal("1.0", problem.Version);
        Assert.Equal(StatusCodes.Status409Conflict, problem.Problem.Status);
        Assert.Equal("command.duplicate", problem.Problem.Extensions["code"]?.ToString());
        Assert.False(string.IsNullOrEmpty(problem.Problem.Extensions["traceId"]?.ToString()));
    }

    [Fact]
    public async Task SendCommandRejectsUnknownMatch()
    {
        await using var connection = BuildConnection();
        await connection.StartAsync();

        var tcs = new TaskCompletionSource<ProblemMessage>();
        connection.On<ProblemMessage>("CommandRejected", p => tcs.TrySetResult(p));

        var command = new SubmitCommandRequest("CheckObjective", Guid.NewGuid(), 1, null, null, null, null);
        await connection.InvokeAsync("SendCommand", Guid.NewGuid(), command);

        var problem = await tcs.Task.WaitAsync(TimeSpan.FromSeconds(1));
        Assert.Equal("1.0", problem.Version);
        Assert.Equal(StatusCodes.Status404NotFound, problem.Problem.Status);
        Assert.Equal("match.not_found", problem.Problem.Extensions["code"]?.ToString());
    }

    [Fact]
    public async Task SendCommandHandlesVanquish()
    {
        var client = factory.CreateClient();
        var create = await client.PostAsJsonAsync("/api/matches", new CreateMatchRequest(["Prince John", "Captain Hook"]));
        var matchId = (await create.Content.ReadFromJsonAsync<CreateMatchResponse>())!.MatchId;
        var state = await client.GetFromJsonAsync<GameStateDto>($"/api/matches/{matchId}/state");
        var playerId = state!.Players[0].Id;

        await using var connection = BuildConnection();
        await connection.StartAsync();
        await connection.InvokeAsync("JoinMatch", matchId);

        var tcs = new TaskCompletionSource<StateMessage>();
        connection.On<StateMessage>("State", s => tcs.TrySetResult(s));

        var command = new SubmitCommandRequest("Vanquish", playerId, 1, null, "Realm", "Hero", null);
        await connection.InvokeAsync("SendCommand", matchId, command);

        var message = await tcs.Task.WaitAsync(TimeSpan.FromSeconds(1));
        Assert.Equal("1.0", message.Version);
    }

    [Fact]
    public async Task JoinMatchRejectsUnknownId()
    {
        await using var connection = BuildConnection();
        await connection.StartAsync();

        var tcs = new TaskCompletionSource<ProblemMessage>();
        connection.On<ProblemMessage>("CommandRejected", p => tcs.TrySetResult(p));

        await connection.InvokeAsync("JoinMatch", Guid.NewGuid());

        var problem = await tcs.Task.WaitAsync(TimeSpan.FromSeconds(1));
        Assert.Equal("1.0", problem.Version);
        Assert.Equal(StatusCodes.Status404NotFound, problem.Problem.Status);
        Assert.Equal("match.not_found", problem.Problem.Extensions["code"]?.ToString());
        Assert.False(string.IsNullOrEmpty(problem.Problem.Extensions["traceId"]?.ToString()));
    }

    [Fact]
    public async Task ReconnectsUsingQueryString()
    {
        var client = factory.CreateClient();
        var create = await client.PostAsJsonAsync("/api/matches", new CreateMatchRequest(["Prince John", "Captain Hook"]));
        var matchId = (await create.Content.ReadFromJsonAsync<CreateMatchResponse>())!.MatchId;

        async Task<StateMessage> ConnectAsync()
        {
            await using var connection = BuildConnection(matchId);
            var tcs = new TaskCompletionSource<StateMessage>();
            connection.On<StateMessage>("State", s => tcs.TrySetResult(s));
            await connection.StartAsync();
            return await tcs.Task.WaitAsync(TimeSpan.FromSeconds(1));
        }

        var first = await ConnectAsync();
        Assert.Equal("1.0", first.Version);
        Assert.Equal(matchId, first.State.MatchId);

        var second = await ConnectAsync();
        Assert.Equal("1.0", second.Version);
        Assert.Equal(matchId, second.State.MatchId);
    }

    [Fact]
    public async Task LeaveMatchStopsStateBroadcast()
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
        await connection2.InvokeAsync("JoinMatch", matchId);
        await connection2.InvokeAsync("LeaveMatch", matchId);

        var tcs = new TaskCompletionSource<StateMessage>();
        connection2.On<StateMessage>("State", s => tcs.TrySetResult(s));

        var command = new SubmitCommandRequest("CheckObjective", playerId, 1, null, null, null, null);
        await connection1.InvokeAsync("SendCommand", matchId, command);

        var completed = await Task.WhenAny(tcs.Task, Task.Delay(TimeSpan.FromMilliseconds(500)));
        Assert.NotSame(tcs.Task, completed);
    }

    private HubConnection BuildConnection(Guid? matchId = null) => new HubConnectionBuilder()
        .WithUrl(matchId is Guid id ? $"{factory.Server.BaseAddress}hub/match?matchId={id}" : $"{factory.Server.BaseAddress}hub/match", options =>
        {
            options.HttpMessageHandlerFactory = _ => factory.Server.CreateHandler();
        })
        .Build();
}

using System.Collections.Concurrent;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Villainous.Engine;
using Villainous.Model;

namespace Villainous.Api;

public class MatchHub : Hub
{
    private readonly ConcurrentDictionary<Guid, GameState> matches;
    private readonly ConcurrentDictionary<Guid, List<DomainEvent>> replays;

    public MatchHub(
        ConcurrentDictionary<Guid, GameState> matches,
        ConcurrentDictionary<Guid, List<DomainEvent>> replays)
    {
        this.matches = matches;
        this.replays = replays;
    }

    public override async Task OnConnectedAsync()
    {
        var httpContext = Context.GetHttpContext();
        if (httpContext is not null &&
            Guid.TryParse(httpContext.Request.Query["matchId"], out var matchId) &&
            matches.TryGetValue(matchId, out var state))
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, matchId.ToString());
            await Clients.Caller.SendAsync("State", state.ToDto());
        }

        await base.OnConnectedAsync();
    }

    public async Task JoinMatch(Guid matchId)
    {
        if (!matches.TryGetValue(matchId, out var state))
        {
            var ctx = Context.GetHttpContext()!;
            await Clients.Caller.SendAsync("CommandRejected", ProblemFactory.CreateDetails(ctx, StatusCodes.Status404NotFound, "match.not_found", "Match not found"));
            return;
        }

        await Groups.AddToGroupAsync(Context.ConnectionId, matchId.ToString());
        await Clients.Caller.SendAsync("State", state.ToDto());
    }

    public async Task SendCommand(Guid matchId, SubmitCommandRequest request)
    {
        if (!matches.TryGetValue(matchId, out var state))
        {
            var ctx = Context.GetHttpContext()!;
            await Clients.Caller.SendAsync("CommandRejected", ProblemFactory.CreateDetails(ctx, StatusCodes.Status404NotFound, "match.not_found", "Match not found"));
            return;
        }

        ICommand? command = request.Type switch
        {
            "Vanquish" when request.Location is { } location && request.Hero is { } hero
                => new VanquishCommand(request.PlayerId, location, hero),
            "Fate" when request.TargetPlayerId is { } target && request.Card is { } card
                => new FateCommand(request.PlayerId, target, card),
            "CheckObjective" => new CheckObjectiveCommand(request.PlayerId),
            _ => null
        };

        if (command is null)
        {
            var ctx = Context.GetHttpContext()!;
            await Clients.Caller.SendAsync("CommandRejected", ProblemFactory.CreateDetails(ctx, StatusCodes.Status400BadRequest, "command.unknown_type", "Unknown command type"));
            return;
        }

        var (newState, events) = GameReducer.Reduce(state, command);
        matches[matchId] = newState;
        replays[matchId].AddRange(events);
        await Clients.Group(matchId.ToString()).SendAsync("State", newState.ToDto());
    }
}

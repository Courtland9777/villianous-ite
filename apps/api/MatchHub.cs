using System.Collections.Concurrent;
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

    public async Task JoinMatch(Guid matchId)
    {
        if (!matches.TryGetValue(matchId, out var state))
        {
            throw new HubException("Match not found");
        }

        await Groups.AddToGroupAsync(Context.ConnectionId, matchId.ToString());
        await Clients.Caller.SendAsync("State", state);
    }

    public async Task SendCommand(Guid matchId, SubmitCommandRequest request)
    {
        if (!matches.TryGetValue(matchId, out var state))
        {
            throw new HubException("Match not found");
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
            throw new HubException("Unknown command");
        }

        var (newState, events) = GameReducer.Reduce(state, command);
        matches[matchId] = newState;
        replays[matchId].AddRange(events);
        await Clients.Group(matchId.ToString()).SendAsync("State", newState);
    }
}

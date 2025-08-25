namespace Villainous.Engine;

public sealed record CheckObjectiveCommand(Guid PlayerId) : ICommand
{
    public IReadOnlyList<DomainEvent> Execute(GameState state)
    {
        ArgumentNullException.ThrowIfNull(state);

        var player = state.Players.FirstOrDefault(p => p.Id == PlayerId);
        if (player is null)
        {
            return Array.Empty<DomainEvent>();
        }

        var achieved = player.Villain switch
        {
            "Prince John" => player.Power >= 20 &&
                player.Locations.Any(l => l.Name == "The Jail" && l.Heroes.Any(h => h.Name == "Robin Hood")),
            _ => false
        };

        return achieved ? new DomainEvent[] { new ObjectiveAchieved(PlayerId) } : Array.Empty<DomainEvent>();
    }
}

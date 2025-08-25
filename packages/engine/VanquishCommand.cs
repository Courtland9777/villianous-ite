namespace Villainous.Engine;

public sealed record VanquishCommand(Guid PlayerId, string Location, string Hero) : ICommand
{
    public IReadOnlyList<DomainEvent> Execute(GameState state)
    {
        ArgumentNullException.ThrowIfNull(state);

        var player = state.Players.FirstOrDefault(p => p.Id == PlayerId);
        if (player is null)
        {
            return Array.Empty<DomainEvent>();
        }

        var location = player.Locations.FirstOrDefault(l => l.Name == Location);
        if (location is null)
        {
            return Array.Empty<DomainEvent>();
        }

        var heroCard = location.Heroes.FirstOrDefault(h => h.Name == Hero);
        if (heroCard is null)
        {
            return Array.Empty<DomainEvent>();
        }

        var totalStrength = location.Allies.Sum(a => a.Strength);
        if (totalStrength < heroCard.Strength)
        {
            return Array.Empty<DomainEvent>();
        }

        return new DomainEvent[] { new HeroVanquished(PlayerId, Location, Hero) };
    }
}

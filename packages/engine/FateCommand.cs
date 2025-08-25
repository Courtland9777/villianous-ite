namespace Villainous.Engine;

public sealed record FateCommand(Guid PlayerId, Guid TargetPlayerId, string Card) : ICommand
{
    public IReadOnlyList<DomainEvent> Execute(GameState state)
    {
        ArgumentNullException.ThrowIfNull(state);

        var player = state.Players.FirstOrDefault(p => p.Id == PlayerId);
        var target = state.Players.FirstOrDefault(p => p.Id == TargetPlayerId);
        if (player is null || target is null || player.Id == target.Id)
        {
            return Array.Empty<DomainEvent>();
        }

        return new DomainEvent[] { new FateCardRevealed(TargetPlayerId, Card) };
    }
}

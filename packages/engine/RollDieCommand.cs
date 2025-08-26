namespace Villainous.Engine;

#pragma warning disable CA5394 // Random is an insecure random number generator
public sealed record RollDieCommand(Guid PlayerId) : ICommand
{
    public IReadOnlyList<DomainEvent> Execute(GameState state)
    {
        ArgumentNullException.ThrowIfNull(state);

        var value = state.Rng.Next(1, 7);
        return new DomainEvent[] { new DieRolled(PlayerId, value) };
    }
}
#pragma warning restore CA5394 // Random is an insecure random number generator


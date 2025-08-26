namespace Villainous.Engine;

public static class GameReducer
{
    public static (GameState State, IReadOnlyList<DomainEvent> Events) Reduce(GameState state, ICommand command)
    {
        ArgumentNullException.ThrowIfNull(state);
        ArgumentNullException.ThrowIfNull(command);

        StateInvariants.Validate(state);
        var events = command.Execute(state);
        StateInvariants.Validate(state);
        return (state, events);
    }
}

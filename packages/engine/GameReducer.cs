namespace Villainous.Engine;

public static class GameReducer
{
    public static (GameState State, IReadOnlyList<DomainEvent> Events) Reduce(GameState state, ICommand command)
    {
        ArgumentNullException.ThrowIfNull(state);
        ArgumentNullException.ThrowIfNull(command);

        var events = command.Execute(state);
        return (state, events);
    }
}

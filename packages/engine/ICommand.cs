namespace Villainous.Engine;

public interface ICommand
{
    IReadOnlyList<DomainEvent> Execute(GameState state);
}

using Villainous.Engine;

namespace Villainous.Engine.Tests;

public class CheckObjectiveCommandTests
{
    [Fact]
    public void Emits_ObjectiveAchieved_For_Prince_John_When_Conditions_Met()
    {
        var jail = new LocationState("The Jail", new[] { new Hero("Robin Hood", 0) }, Array.Empty<Ally>());
        var player = new PlayerState(Guid.NewGuid(), "Prince John", 20, new[] { jail });
        var state = new GameState(Guid.NewGuid(), new[] { player }, 0, 0, new Random(0));

        var command = new CheckObjectiveCommand(player.Id);
        var events = command.Execute(state);

        Assert.Contains(new ObjectiveAchieved(player.Id), events);
    }

    [Fact]
    public void Does_Not_Emit_When_Conditions_Not_Met()
    {
        var jail = new LocationState("The Jail", Array.Empty<Hero>(), Array.Empty<Ally>());
        var player = new PlayerState(Guid.NewGuid(), "Prince John", 19, new[] { jail });
        var state = new GameState(Guid.NewGuid(), new[] { player }, 0, 0, new Random(0));

        var command = new CheckObjectiveCommand(player.Id);
        var events = command.Execute(state);

        Assert.Empty(events);
    }
}

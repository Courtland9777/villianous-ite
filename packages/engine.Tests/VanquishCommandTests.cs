using Villainous.Engine;

namespace Villainous.Engine.Tests;

public class VanquishCommandTests
{
    [Fact]
    public void Emits_HeroVanquished_When_Allies_Stronger()
    {
        var hero = new Hero("Hero", 3);
        var allies = new[] { new Ally("Ally1", 2), new Ally("Ally2", 2) };
        var location = new LocationState("Realm", new[] { hero }, allies);
        var player = new PlayerState(Guid.NewGuid(), "Villain", 0, new[] { location });
        var state = new GameState(Guid.NewGuid(), new[] { player }, 0, 0);

        var command = new VanquishCommand(player.Id, "Realm", "Hero");
        var events = command.Execute(state);

        Assert.Contains(new HeroVanquished(player.Id, "Realm", "Hero"), events);
    }
}

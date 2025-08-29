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
        var state = new GameState(Guid.NewGuid(), new[] { player }, 0, 0, new Random(0));

        var command = new VanquishCommand(player.Id, "Realm", "Hero");
        var events = command.Execute(state);

        Assert.Contains(new HeroVanquished(player.Id, "Realm", "Hero"), events);
    }

    [Fact]
    public void ReturnsEmpty_When_Player_NotFound()
    {
        var state = new GameState(Guid.NewGuid(), Array.Empty<PlayerState>(), 0, 0, new Random(0));
        var command = new VanquishCommand(Guid.NewGuid(), "Realm", "Hero");
        Assert.Empty(command.Execute(state));
    }

    [Fact]
    public void ReturnsEmpty_When_Location_NotFound()
    {
        var player = new PlayerState(Guid.NewGuid(), "Villain", 0, Array.Empty<LocationState>());
        var state = new GameState(Guid.NewGuid(), new[] { player }, 0, 0, new Random(0));
        var command = new VanquishCommand(player.Id, "Missing", "Hero");
        Assert.Empty(command.Execute(state));
    }

    [Fact]
    public void ReturnsEmpty_When_Hero_NotFound()
    {
        var location = new LocationState("Realm", Array.Empty<Hero>(), Array.Empty<Ally>());
        var player = new PlayerState(Guid.NewGuid(), "Villain", 0, new[] { location });
        var state = new GameState(Guid.NewGuid(), new[] { player }, 0, 0, new Random(0));
        var command = new VanquishCommand(player.Id, "Realm", "Hero");
        Assert.Empty(command.Execute(state));
    }

    [Fact]
    public void ReturnsEmpty_When_Allies_Too_Weak()
    {
        var hero = new Hero("Hero", 5);
        var allies = new[] { new Ally("Ally1", 2) };
        var location = new LocationState("Realm", new[] { hero }, allies);
        var player = new PlayerState(Guid.NewGuid(), "Villain", 0, new[] { location });
        var state = new GameState(Guid.NewGuid(), new[] { player }, 0, 0, new Random(0));
        var command = new VanquishCommand(player.Id, "Realm", "Hero");
        Assert.Empty(command.Execute(state));
    }
}

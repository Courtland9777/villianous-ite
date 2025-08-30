using System;
using Xunit;
using Villainous.Engine;

public class StateInvariantTests
{
    [Fact]
    public void Reduce_Throws_When_Power_Negative()
    {
        var player = new PlayerState(Guid.NewGuid(), "Villain", -1, Array.Empty<LocationState>());
        var state = new GameState(Guid.NewGuid(), new[] { player }, 0, 0, new Random(1));
        Assert.Throws<InvalidOperationException>(() => GameReducer.Reduce(state, new RollDieCommand(player.Id)));
    }

    [Fact]
    public void Reduce_Throws_When_DeckCount_Negative()
    {
        var player = new PlayerState(Guid.NewGuid(), "Villain", 0, Array.Empty<LocationState>(), -1);
        var state = new GameState(Guid.NewGuid(), new[] { player }, 0, 0, new Random(1));
        Assert.Throws<InvalidOperationException>(() => GameReducer.Reduce(state, new RollDieCommand(player.Id)));
    }

    [Fact]
    public void Reduce_Throws_When_Dangling_Hero()
    {
        var location = new LocationState("Loc", new Hero?[] { null }!, Array.Empty<Ally>());
        var player = new PlayerState(Guid.NewGuid(), "Villain", 0, new[] { location });
        var state = new GameState(Guid.NewGuid(), new[] { player }, 0, 0, new Random(1));
        Assert.Throws<InvalidOperationException>(() => GameReducer.Reduce(state, new RollDieCommand(player.Id)));
    }

    [Fact]
    public void Reduce_Throws_When_FateDeck_Negative()
    {
        var player = new PlayerState(Guid.NewGuid(), "Villain", 0, Array.Empty<LocationState>(), 0, -1);
        var state = new GameState(Guid.NewGuid(), new[] { player }, 0, 0, new Random(1));
        Assert.Throws<InvalidOperationException>(() => GameReducer.Reduce(state, new RollDieCommand(player.Id)));
    }

    [Fact]
    public void Reduce_Throws_When_Locations_Null()
    {
        var player = new PlayerState(Guid.NewGuid(), "Villain", 0, null!, 0, 0);
        var state = new GameState(Guid.NewGuid(), new[] { player }, 0, 0, new Random(1));
        Assert.Throws<InvalidOperationException>(() => GameReducer.Reduce(state, new RollDieCommand(player.Id)));
    }

    [Fact]
    public void Reduce_Throws_When_Location_Null()
    {
        var player = new PlayerState(Guid.NewGuid(), "Villain", 0, new LocationState[] { null! });
        var state = new GameState(Guid.NewGuid(), new[] { player }, 0, 0, new Random(1));
        Assert.Throws<InvalidOperationException>(() => GameReducer.Reduce(state, new RollDieCommand(player.Id)));
    }

    [Fact]
    public void Reduce_Throws_When_Heroes_Null()
    {
        var location = new LocationState("Loc", null!, Array.Empty<Ally>());
        var player = new PlayerState(Guid.NewGuid(), "Villain", 0, new[] { location });
        var state = new GameState(Guid.NewGuid(), new[] { player }, 0, 0, new Random(1));
        Assert.Throws<InvalidOperationException>(() => GameReducer.Reduce(state, new RollDieCommand(player.Id)));
    }

    [Fact]
    public void Reduce_Throws_When_Allies_Null()
    {
        var location = new LocationState("Loc", Array.Empty<Hero>(), null!);
        var player = new PlayerState(Guid.NewGuid(), "Villain", 0, new[] { location });
        var state = new GameState(Guid.NewGuid(), new[] { player }, 0, 0, new Random(1));
        Assert.Throws<InvalidOperationException>(() => GameReducer.Reduce(state, new RollDieCommand(player.Id)));
    }
}

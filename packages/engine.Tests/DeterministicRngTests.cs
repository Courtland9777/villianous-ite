using Villainous.Engine;

namespace Villainous.Engine.Tests;

public class DeterministicRngTests
{
    [Fact]
    public void Same_Seed_Produces_Same_Event_Sequence()
    {
        var player = new PlayerState(Guid.NewGuid(), "Villain", 0, Array.Empty<LocationState>());
        var seed = 123;
        var state1 = new GameState(Guid.NewGuid(), new[] { player }, 0, 0, new Random(seed));
        var state2 = new GameState(Guid.NewGuid(), new[] { player }, 0, 0, new Random(seed));

        var commands = Enumerable.Repeat<ICommand>(new RollDieCommand(player.Id), 5).ToList();

        var events1 = new List<DomainEvent>();
        foreach (var command in commands)
        {
            var (next, evts) = GameReducer.Reduce(state1, command);
            state1 = next;
            events1.AddRange(evts);
        }

        var events2 = new List<DomainEvent>();
        foreach (var command in commands)
        {
            var (next, evts) = GameReducer.Reduce(state2, command);
            state2 = next;
            events2.AddRange(evts);
        }

        Assert.Equal(events1, events2);
    }

    [Fact]
    public void Different_Seeds_Produce_Different_Event_Sequence()
    {
        var player = new PlayerState(Guid.NewGuid(), "Villain", 0, Array.Empty<LocationState>());
        var state1 = new GameState(Guid.NewGuid(), new[] { player }, 0, 0, new Random(123));
        var state2 = new GameState(Guid.NewGuid(), new[] { player }, 0, 0, new Random(456));

        var commands = Enumerable.Repeat<ICommand>(new RollDieCommand(player.Id), 5).ToList();

        var events1 = new List<DomainEvent>();
        foreach (var command in commands)
        {
            var (next, evts) = GameReducer.Reduce(state1, command);
            state1 = next;
            events1.AddRange(evts);
        }

        var events2 = new List<DomainEvent>();
        foreach (var command in commands)
        {
            var (next, evts) = GameReducer.Reduce(state2, command);
            state2 = next;
            events2.AddRange(evts);
        }

        Assert.NotEqual(events1, events2);
    }
}


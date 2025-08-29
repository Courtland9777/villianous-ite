using FsCheck;
using FsCheck.Xunit;
using System.Text.Json;
using Villainous.Engine;

namespace Villainous.Engine.Tests;

public class DeterministicRngTests
{
    [Property(MaxTest = 20)]
    public void Roll_Die_Is_Deterministic(int seed, PositiveInt length)
    {
        var player = new PlayerState(Guid.NewGuid(), "Villain", 0, Array.Empty<LocationState>());
        var count = length.Get % 10 + 1;

        var state1 = new GameState(Guid.NewGuid(), new[] { player }, 0, 0, new Random(seed));
        var state2 = new GameState(Guid.NewGuid(), new[] { player }, 0, 0, new Random(seed));

        var commands = Enumerable.Repeat<ICommand>(new RollDieCommand(player.Id), count);

        var events1 = ExecuteCommands(state1, commands);
        var events2 = ExecuteCommands(state2, commands);

        Assert.Equal(events1, events2);
    }

    [Fact]
    public void Roll_Die_Matches_Golden_Fixture()
    {
        var path = Path.Combine(AppContext.BaseDirectory, "Fixtures", "roll_die_seed123.json");
        var json = File.ReadAllText(path);
        var fixture = JsonSerializer.Deserialize<DieRollFixture>(json)!;

        var player = new PlayerState(Guid.NewGuid(), "Villain", 0, Array.Empty<LocationState>());
        var state = new GameState(Guid.NewGuid(), new[] { player }, 0, 0, new Random(fixture.Seed));

        var events = ExecuteCommands(state, fixture.Rolls.Select(_ => new RollDieCommand(player.Id)));
        var rolls = events.Cast<DieRolled>().Select(e => e.Value).ToArray();

        Assert.Equal(fixture.Rolls, rolls);
    }

    private static List<DomainEvent> ExecuteCommands(GameState state, IEnumerable<ICommand> commands)
    {
        var events = new List<DomainEvent>();
        foreach (var command in commands)
        {
            var (next, evts) = GameReducer.Reduce(state, command);
            state = next;
            events.AddRange(evts);
        }

        return events;
    }

    private sealed record DieRollFixture(int Seed, int[] Rolls);
}


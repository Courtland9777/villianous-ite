using Villainous.Engine;

namespace Villainous.Engine.Tests;

public class FateCommandTests
{
    [Fact]
    public void Emits_FateCardRevealed_For_Target_Player()
    {
        var player1 = new PlayerState(Guid.NewGuid(), "Villain1", 0, Array.Empty<LocationState>());
        var player2 = new PlayerState(Guid.NewGuid(), "Villain2", 0, Array.Empty<LocationState>());
        var state = new GameState(Guid.NewGuid(), new[] { player1, player2 }, 0, 0);

        var command = new FateCommand(player1.Id, player2.Id, "Hero");
        var events = command.Execute(state);

        Assert.Contains(new FateCardRevealed(player2.Id, "Hero"), events);
    }

    [Fact]
    public void Does_Not_Emit_When_Targeting_Self()
    {
        var player = new PlayerState(Guid.NewGuid(), "Villain", 0, Array.Empty<LocationState>());
        var state = new GameState(Guid.NewGuid(), new[] { player }, 0, 0);

        var command = new FateCommand(player.Id, player.Id, "Hero");
        var events = command.Execute(state);

        Assert.Empty(events);
    }
}

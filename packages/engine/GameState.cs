namespace Villainous.Engine;

public sealed record GameState(
    Guid MatchId,
    IReadOnlyList<PlayerState> Players,
    int CurrentPlayerIndex,
    int Turn
);

namespace Villainous.Model;

public sealed record GameStateDto(
    Guid MatchId,
    IReadOnlyList<PlayerStateDto> Players,
    int CurrentPlayerIndex,
    int Turn
);

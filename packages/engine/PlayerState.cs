namespace Villainous.Engine;

public sealed record PlayerState(
    Guid Id,
    string Villain,
    int Power,
    IReadOnlyList<LocationState> Locations,
    int VillainDeckCount = 0,
    int FateDeckCount = 0
);

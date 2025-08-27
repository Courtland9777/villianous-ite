using Villainous.Engine;

namespace Villainous.Model;

public sealed record PlayerStateDto(
    Guid Id,
    string Villain,
    int Power,
    IReadOnlyList<LocationState> Locations,
    int VillainDeckCount,
    int FateDeckCount
);

namespace Villainous.Engine;

public sealed record PlayerState(
    Guid Id,
    string Villain,
    int Power,
    IReadOnlyList<LocationState> Locations
);

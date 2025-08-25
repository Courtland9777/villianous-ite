namespace Villainous.Engine;

public sealed record LocationState(
    string Name,
    IReadOnlyList<Hero> Heroes,
    IReadOnlyList<Ally> Allies
);

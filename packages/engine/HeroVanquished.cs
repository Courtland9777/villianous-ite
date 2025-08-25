namespace Villainous.Engine;

public sealed record HeroVanquished(Guid PlayerId, string Location, string Hero) : DomainEvent;

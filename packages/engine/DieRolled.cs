namespace Villainous.Engine;

public sealed record DieRolled(Guid PlayerId, int Value) : DomainEvent;


namespace Villainous.Engine;

public sealed record ObjectiveAchieved(Guid PlayerId) : DomainEvent;

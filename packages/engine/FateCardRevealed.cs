namespace Villainous.Engine;

public sealed record FateCardRevealed(Guid PlayerId, string Card) : DomainEvent;

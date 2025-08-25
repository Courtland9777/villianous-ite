namespace Villainous.Model;

using System;

public record SubmitCommandRequest(
    string Type,
    Guid PlayerId,
    Guid? TargetPlayerId,
    string? Location,
    string? Hero,
    string? Card);

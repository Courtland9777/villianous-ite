namespace Villainous.Model;

using System;
using System.ComponentModel.DataAnnotations;

public record SubmitCommandRequest(
    [property: Required, MinLength(1)] string Type,
    Guid PlayerId,
    int ClientSeq,
    Guid? TargetPlayerId,
    string? Location,
    string? Hero,
    string? Card);

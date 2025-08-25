using System;
using System.Collections.Concurrent;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Villainous.Engine;
using Villainous.Model;

var builder = WebApplication.CreateBuilder(args);
var matches = new ConcurrentDictionary<Guid, GameState>();
var replays = new ConcurrentDictionary<Guid, List<DomainEvent>>();
var app = builder.Build();

app.MapPost("/api/matches", (CreateMatchRequest request) =>
{
    var matchId = Guid.NewGuid();
    var players = request.Villains
        .Select(v => new PlayerState(Guid.NewGuid(), v, 0, Array.Empty<LocationState>()))
        .ToList();
    var state = new GameState(matchId, players, 0, 0);
    matches[matchId] = state;
    replays[matchId] = new List<DomainEvent>();
    return Results.Json(new CreateMatchResponse(matchId), statusCode: StatusCodes.Status201Created);
});

app.MapGet("/api/matches/{id:guid}/state", (Guid id) =>
    matches.TryGetValue(id, out var state)
        ? Results.Json(state)
        : Results.NotFound());

app.MapGet("/api/matches/{id:guid}/replay", (Guid id) =>
    replays.TryGetValue(id, out var events)
        ? Results.Json(events)
        : Results.NotFound());

app.Run();

public partial class Program;

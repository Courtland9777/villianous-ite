using System;
using System.Collections.Concurrent;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;
using Serilog;
using Villainous.Engine;
using Villainous.Model;
using Villainous.Api;

var builder = WebApplication.CreateBuilder(args);
builder.Host.UseSerilog((ctx, services, cfg) => cfg
    .ReadFrom.Configuration(ctx.Configuration)
    .ReadFrom.Services(services)
    .Enrich.FromLogContext());

builder.Services.AddHealthChecks();

builder.Services.AddOpenTelemetry()
    .WithTracing(b => b
        .AddAspNetCoreInstrumentation()
        .AddHttpClientInstrumentation()
        .AddOtlpExporter())
    .WithMetrics(b => b
        .AddAspNetCoreInstrumentation()
        .AddHttpClientInstrumentation()
        .AddRuntimeInstrumentation()
        .AddOtlpExporter());

builder.Services.AddSingleton<ConcurrentDictionary<Guid, GameState>>();
builder.Services.AddSingleton<ConcurrentDictionary<Guid, List<DomainEvent>>>();
builder.Services.AddSignalR();

var app = builder.Build();
app.UseSerilogRequestLogging();

var matches = app.Services.GetRequiredService<ConcurrentDictionary<Guid, GameState>>();
var replays = app.Services.GetRequiredService<ConcurrentDictionary<Guid, List<DomainEvent>>>();

app.MapHealthChecks("/healthz/live");
app.MapHealthChecks("/ready");

app.MapPost("/api/matches", (CreateMatchRequest request) =>
{
    var matchId = Guid.NewGuid();
    var players = request.Villains
        .Select(v => new PlayerState(Guid.NewGuid(), v, 0, Array.Empty<LocationState>()))
        .ToList();
    var state = new GameState(matchId, players, 0, 0, new Random(0));
    matches[matchId] = state;
    replays[matchId] = new List<DomainEvent>();
    return Results.Json(new CreateMatchResponse(matchId), statusCode: StatusCodes.Status201Created);
});

app.MapGet("/api/matches/{id:guid}/state", (Guid id) =>
    matches.TryGetValue(id, out var state)
        ? Results.Json(state)
        : Results.Problem(title: "Match not found", statusCode: StatusCodes.Status404NotFound, type: "match.not_found"));

app.MapGet("/api/matches/{id:guid}/replay", (Guid id) =>
    replays.TryGetValue(id, out var events)
        ? Results.Json(events)
        : Results.Problem(title: "Match not found", statusCode: StatusCodes.Status404NotFound, type: "match.not_found"));

app.MapPost("/api/matches/{id:guid}/commands", (Guid id, SubmitCommandRequest request) =>
{
    if (!matches.TryGetValue(id, out var state))
    {
        return Results.Problem(title: "Match not found", statusCode: StatusCodes.Status404NotFound, type: "match.not_found");
    }

    ICommand? command = request.Type switch
    {
        "Vanquish" when request.Location is { } location && request.Hero is { } hero
            => new VanquishCommand(request.PlayerId, location, hero),
        "Fate" when request.TargetPlayerId is { } target && request.Card is { } card
            => new FateCommand(request.PlayerId, target, card),
        "CheckObjective" => new CheckObjectiveCommand(request.PlayerId),
        _ => null
    };

    if (command is null)
    {
        return Results.Problem(title: "Unknown command type", statusCode: StatusCodes.Status400BadRequest, type: "rules.illegal_action");
    }

    var (newState, events) = GameReducer.Reduce(state, command);
    matches[id] = newState;
    replays[id].AddRange(events);
    return Results.Json(new SubmitCommandResponse(true));
});

app.MapHub<MatchHub>("/hub/match");

app.Run();

public partial class Program;

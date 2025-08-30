using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;
using Serilog;
using Serilog.Context;
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
builder.Services.AddSingleton<ConcurrentDictionary<(Guid, Guid, int), bool>>();
builder.Services.AddSignalR();

var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>();
if (allowedOrigins is { Length: > 0 })
{
    builder.Services.AddCors(options =>
        options.AddPolicy("frontend", policy => policy
            .WithOrigins(allowedOrigins)
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials()));
}

var app = builder.Build();
app.UseSerilogRequestLogging();

if (allowedOrigins is { Length: > 0 })
{
    app.UseCors("frontend");
}

app.Use(async (ctx, next) =>
{
    var traceId = Activity.Current?.TraceId.ToString() ?? ctx.TraceIdentifier;
    using (LogContext.PushProperty("TraceId", traceId))
    {
        await next();
    }
});

var matches = app.Services.GetRequiredService<ConcurrentDictionary<Guid, GameState>>();
var replays = app.Services.GetRequiredService<ConcurrentDictionary<Guid, List<DomainEvent>>>();
var processed = app.Services.GetRequiredService<ConcurrentDictionary<(Guid, Guid, int), bool>>();

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

app.MapGet("/api/matches/{id:guid}/state", (HttpContext ctx, Guid id) =>
{
    using var _ = LogContext.PushProperty("MatchId", id);
    return matches.TryGetValue(id, out var state)
        ? Results.Json(state.ToDto())
        : ProblemFactory.Create(ctx, StatusCodes.Status404NotFound, "match.not_found", "Match not found");
});

app.MapGet("/api/matches/{id:guid}/replay", (HttpContext ctx, Guid id) =>
{
    using var _ = LogContext.PushProperty("MatchId", id);
    return replays.TryGetValue(id, out var events)
        ? Results.Json(events)
        : ProblemFactory.Create(ctx, StatusCodes.Status404NotFound, "match.not_found", "Match not found");
});

app.MapPost("/api/matches/{id:guid}/commands", (HttpContext ctx, Guid id, SubmitCommandRequest request) =>
{
    using var matchLog = LogContext.PushProperty("MatchId", id);
    using var playerLog = LogContext.PushProperty("PlayerId", request.PlayerId);

    if (!matches.TryGetValue(id, out var state))
    {
        return ProblemFactory.Create(ctx, StatusCodes.Status404NotFound, "match.not_found", "Match not found");
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
        return ProblemFactory.Create(ctx, StatusCodes.Status400BadRequest, "command.unknown_type", "Unknown command type");
    }

    var key = (id, request.PlayerId, request.ClientSeq);
    if (!processed.TryAdd(key, true))
    {
        return ProblemFactory.Create(ctx, StatusCodes.Status409Conflict, "command.duplicate", "Duplicate command");
    }

    var (newState, events) = GameReducer.Reduce(state, command);
    matches[id] = newState;
    replays[id].AddRange(events);
    Log.Information("Command processed");
    return Results.Json(new SubmitCommandResponse(true));
});

app.MapHub<MatchHub>("/hub/match");

app.Run();

public partial class Program;

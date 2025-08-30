using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Linq;
using System.Text.Json;
using System.Threading.RateLimiting;
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

if (!builder.Environment.IsEnvironment("Testing"))
{
    builder.Services.AddRateLimiter(options =>
    {
        options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
        options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
            RateLimitPartition.GetFixedWindowLimiter(
                context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                _ => new FixedWindowRateLimiterOptions
                {
                    PermitLimit = 5,
                    Window = TimeSpan.FromMinutes(1),
                    QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                    QueueLimit = 0
                }));
    });
}

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

app.Use(async (ctx, next) =>
{
    IDisposable? matchScope = null;
    IDisposable? playerScope = null;

    if (ctx.Request.Path.Value is { } path)
    {
        var segments = path.Split('/', StringSplitOptions.RemoveEmptyEntries);
        if (segments.Length >= 3 && segments[0] == "api" && segments[1] == "matches" && Guid.TryParse(segments[2], out var matchId))
        {
            matchScope = LogContext.PushProperty("MatchId", matchId);

            if (segments.Length >= 4 && segments[3] == "commands" && string.Equals(ctx.Request.Method, HttpMethods.Post, StringComparison.OrdinalIgnoreCase))
            {
                ctx.Request.EnableBuffering();
                var command = await JsonSerializer.DeserializeAsync<SubmitCommandRequest>(ctx.Request.Body, new JsonSerializerOptions { PropertyNameCaseInsensitive = true }, ctx.RequestAborted);
                ctx.Request.Body.Position = 0;
                if (command is not null)
                {
                    playerScope = LogContext.PushProperty("PlayerId", command.PlayerId);
                }
            }
        }
    }

    try
    {
        await next();
    }
    finally
    {
        playerScope?.Dispose();
        matchScope?.Dispose();
    }
});

app.UseSerilogRequestLogging();

if (!app.Environment.IsEnvironment("Testing"))
{
    app.UseRateLimiter();
}

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

app.MapPost("/api/matches", (HttpContext ctx, CreateMatchRequest request) =>
{
    var villains = request.Villains
        .Select(v => v.Trim())
        .Where(v => !string.IsNullOrWhiteSpace(v))
        .ToList();
    if (villains.Count < 2 || villains.Count != request.Villains.Count)
    {
        return ProblemFactory.Create(ctx, StatusCodes.Status400BadRequest, "match.invalid_villains", "Invalid villains");
    }

    var matchId = Guid.NewGuid();
    var players = villains
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

    var type = request.Type.Trim();
    var location = request.Location?.Trim();
    var hero = request.Hero?.Trim();
    var card = request.Card?.Trim();

    if (string.IsNullOrWhiteSpace(type))
    {
        return ProblemFactory.Create(ctx, StatusCodes.Status400BadRequest, "command.invalid_type", "Invalid command type");
    }

    if (request.PlayerId == Guid.Empty)
    {
        return ProblemFactory.Create(ctx, StatusCodes.Status400BadRequest, "command.invalid_player", "Invalid player id");
    }

    if (request.ClientSeq < 0)
    {
        return ProblemFactory.Create(ctx, StatusCodes.Status400BadRequest, "command.invalid_client_seq", "Invalid client sequence");
    }

    if (!matches.TryGetValue(id, out var state))
    {
        return ProblemFactory.Create(ctx, StatusCodes.Status404NotFound, "match.not_found", "Match not found");
    }

    ICommand? command = type switch
    {
        "Vanquish" when location is { } l && hero is { } h
            => new VanquishCommand(request.PlayerId, l, h),
        "Fate" when request.TargetPlayerId is { } target && target != Guid.Empty && card is { } c
            => new FateCommand(request.PlayerId, target, c),
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

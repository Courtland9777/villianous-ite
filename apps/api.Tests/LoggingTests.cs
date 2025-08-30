using System.Collections.Concurrent;
using System.Linq;
using System.Net.Http.Json;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Core;
using Serilog.Events;
using Villainous.Model;
using Xunit;

namespace Villainous.Api.Tests;

public class LoggingTests : IClassFixture<LoggingWebApplicationFactory>
{
    private readonly LoggingWebApplicationFactory factory;

    public LoggingTests(LoggingWebApplicationFactory factory)
    {
        this.factory = factory;
    }

    [Fact]
    public async Task RequestLogIncludesMatchAndPlayer()
    {
        var client = factory.CreateClient();
        var create = await client.PostAsJsonAsync("/api/matches", new CreateMatchRequest(["Prince John", "Captain Hook"]));
        var match = await create.Content.ReadFromJsonAsync<CreateMatchResponse>();
        var state = await client.GetFromJsonAsync<GameStateDto>($"/api/matches/{match!.MatchId}/state");
        var player = state!.Players[0].Id;
        var target = state.Players[1].Id;

        var command = new SubmitCommandRequest("Fate", player, 1, target, null, null, "Ariel");
        await client.PostAsJsonAsync($"/api/matches/{match.MatchId}/commands", command);

        var evt = factory.Sink.Events.FirstOrDefault(e =>
            e.Properties.ContainsKey("MatchId") && e.Properties.ContainsKey("PlayerId"));
        Assert.NotNull(evt);
        Assert.Equal(match.MatchId.ToString(), ((ScalarValue)evt!.Properties["MatchId"]).Value?.ToString());
        Assert.Equal(player.ToString(), ((ScalarValue)evt.Properties["PlayerId"]).Value?.ToString());
        Assert.All(evt.Properties.Values, v => Assert.DoesNotContain("Ariel", v.ToString()));
    }
}

public class LoggingWebApplicationFactory : TestingWebApplicationFactory
{
    public InMemorySink Sink { get; } = new();

    protected override IHost CreateHost(IHostBuilder builder)
    {
        builder.UseSerilog((ctx, services, cfg) =>
            cfg.ReadFrom.Configuration(ctx.Configuration)
               .Enrich.FromLogContext()
               .WriteTo.Sink(Sink));
        return base.CreateHost(builder);
    }
}

public class InMemorySink : ILogEventSink
{
    public ConcurrentBag<LogEvent> Events { get; } = new();
    public void Emit(LogEvent logEvent) => Events.Add(logEvent);
}

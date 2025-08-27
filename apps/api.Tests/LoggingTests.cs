using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
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

        LogEvent? evt = null;
        for (var i = 0; i < 10 && evt == null; i++)
        {
            evt = factory.Sink.Events.FirstOrDefault(e =>
                e.Properties.ContainsKey("MatchId") && e.Properties.ContainsKey("PlayerId"));
            if (evt == null)
            {
                await Task.Delay(50);
            }
        }

        Assert.NotNull(evt);
        Assert.Equal(match.MatchId.ToString(), ((ScalarValue)evt!.Properties["MatchId"]).Value?.ToString());
        Assert.Equal(player.ToString(), ((ScalarValue)evt.Properties["PlayerId"]).Value?.ToString());
        Assert.All(evt.Properties.Values, v => Assert.DoesNotContain("Ariel", v.ToString()));
    }
}

public class LoggingWebApplicationFactory : WebApplicationFactory<Program>
{
    public InMemorySink Sink { get; } = new();

    protected override IHost CreateHost(IHostBuilder builder)
    {
        Log.Logger = new LoggerConfiguration()
            .Enrich.FromLogContext()
            .WriteTo.Sink(Sink)
            .CreateLogger();
        builder.UseSerilog();
        return base.CreateHost(builder);
    }
}

public class InMemorySink : ILogEventSink
{
    public System.Collections.Concurrent.ConcurrentBag<LogEvent> Events { get; } = new();
    public void Emit(LogEvent logEvent) => Events.Add(logEvent);
}

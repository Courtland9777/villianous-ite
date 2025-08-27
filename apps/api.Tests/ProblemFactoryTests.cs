using System.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Xunit;

namespace Villainous.Api.Tests;

public class ProblemFactoryTests
{
    [Fact]
    public void UsesActivityTraceId()
    {
        var context = new DefaultHttpContext();
        using var activity = new Activity("test").Start();

        var problem = ProblemFactory.CreateDetails(context, StatusCodes.Status500InternalServerError, "err", "Error");

        Assert.Equal(activity.TraceId.ToString(), problem.Extensions["traceId"]?.ToString());
    }
}


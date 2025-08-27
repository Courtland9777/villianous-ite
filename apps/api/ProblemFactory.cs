using System.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Villainous.Api;

public static class ProblemFactory
{
    public static ProblemDetails CreateDetails(HttpContext context, int statusCode, string code, string title)
    {
        var problem = new ProblemDetails
        {
            Title = title,
            Status = statusCode
        };
        problem.Extensions["code"] = code;
        problem.Extensions["traceId"] = Activity.Current?.TraceId.ToString() ?? context.TraceIdentifier;
        return problem;
    }

    public static IResult Create(HttpContext context, int statusCode, string code, string title) =>
        Results.Problem(CreateDetails(context, statusCode, code, title));
}

using System;
using Microsoft.AspNetCore.Http;
using Villainous.Model;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

app.MapPost("/api/matches", (CreateMatchRequest request) =>
{
    var matchId = Guid.NewGuid();
    return Results.Json(new CreateMatchResponse(matchId), statusCode: StatusCodes.Status201Created);
});

app.Run();

public partial class Program;

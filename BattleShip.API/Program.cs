using BattleShip.Models;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSingleton<WarService>();
builder.Services.AddCors();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors(c =>
{
    c.AllowAnyHeader();
    c.AllowAnyMethod();
    c.AllowAnyOrigin();
});

app.MapGet("/war/{id}", Results<NotFound, JsonHttpResult<WarDto>> (WarService warService, [FromRoute] Guid id) =>
{
    if (!warService.Wars.ContainsKey(id))
    {
        return TypedResults.NotFound();
    }

    War war = warService.Wars[id];

    return TypedResults.Json(war.ToDto());
})
.WithName("GetWar")
.WithOpenApi();

app.MapPost("/war/beam/{id}", Results<NotFound, JsonHttpResult<BeamResponseDto>> (WarService warService, [FromRoute] Guid id, [FromBody] BeamActionDto beamAction) =>
{
    if (!warService.Wars.ContainsKey(id))
    {
        return TypedResults.NotFound();
    }

    War war = warService.Wars[id];

    return TypedResults.Json(war.Beam(beamAction));
}).WithName("Beam")
.WithOpenApi();

app.MapPost("/war", (WarService warService) =>
{
    War war = new War();
    warService.Wars.Add(war.Id, war);
    // return game.PlayerGrid.ToJaggedArray();
    return TypedResults.Json(war.ToDto());
})
.WithName("CreateWar")
.WithOpenApi();

app.Run();


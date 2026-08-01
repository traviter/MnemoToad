using Microsoft.EntityFrameworkCore;
using MnemoToad.Api.Data;
using MnemoToad.Api.Models;
using System.ComponentModel.DataAnnotations;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("Default")).UseSnakeCaseNamingConvention());

builder.Services.AddScoped<NodeTypeService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapGet("/health", () => Results.Ok(new { status = "pass" }));

app.MapGet("/countries", async (AppDbContext db) =>
    await db.Country.OrderBy(c => c.Name).ToListAsync());

app.MapGet("/countries/{isoCode}", async (string isoCode, AppDbContext db) =>
    await db.Country.FirstOrDefaultAsync(c => c.IsoCode == isoCode)
        is Country country ? Results.Ok(country) : Results.NotFound());

app.MapGet("/nodeTypes", async (NodeTypeService service) =>
    await service.GetAllAsync());

app.MapGet("/nodeTypes/{id:guid}", async (Guid id, NodeTypeService service) =>
    await service.GetByIdAsync(id) is NodeType nodeType ? Results.Ok(nodeType) : Results.NotFound());

app.MapPost("/nodeTypes", async (NodeTypeRequest request, NodeTypeService service) =>
{
    try
    {
        var created = await service.CreateAsync(request.Name, request.Description);
        return Results.Created($"/nodeTypes/{created.Id}", created);
    }
    catch (ValidationException ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
});

app.MapPut("/nodeTypes/{id:guid}", async (Guid id, NodeTypeRequest request, NodeTypeService service) =>
{
    try
    {
        var updated = await service.UpdateAsync(id, request.Name, request.Description);
        return updated is not null ? Results.Ok(updated) : Results.NotFound();
    }
    catch (ValidationException ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
});

app.MapDelete("/nodeTypes/{id:guid}", async (Guid id, NodeTypeService service) =>
    await service.DeleteAsync(id) ? Results.NoContent() : Results.NotFound());

app.Run();

record NodeTypeRequest(string Name, string? Description);
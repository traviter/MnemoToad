using Microsoft.EntityFrameworkCore;
using MnemoToad.Api.Data;
using MnemoToad.Api.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("Default")).UseSnakeCaseNamingConvention());

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

app.Run();
using ConcertRadar.Backend.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.AddDbContext<ConcertRadarDbContext>(options =>
    options.UseSqlite($"Data Source={ConcertRadarDatabase.Resolve(builder.Environment.ContentRootPath)}"));

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<ConcertRadarDbContext>();
    dbContext.Database.Migrate();
}

app.UseAuthorization();

app.MapControllers();

app.Run();

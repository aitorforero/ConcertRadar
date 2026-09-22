using ConcertRadar.Backend.Infrastructure.Persistence;
using ConcertRadar.Backend.Scraper;
using ConcertRadar.Backend.Scraper.Importing;
using ConcertRadar.Backend.Scraper.Scraping;
using ConcertRadar.Backend.Scraper.Scraping.MariskalRock;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

var builder = Host.CreateApplicationBuilder(args);
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Services.Configure<HtmlSourceOptions>(builder.Configuration.GetSection("Scraping"));
builder.Services.AddDbContext<ConcertRadarDbContext>(options =>
	options.UseSqlite($"Data Source={ConcertRadarDatabase.Resolve(builder.Environment.ContentRootPath)}"));
builder.Services.AddHttpClient<MariskalRockConcertSource>(client =>
{
	client.Timeout = TimeSpan.FromSeconds(30);
	client.DefaultRequestHeaders.UserAgent.ParseAdd("ConcertRadar/1.0");
});
builder.Services.AddScoped<ConcertImporter>();
builder.Services.AddSingleton<IConcertSource>(services =>
	services.GetRequiredService<MariskalRockConcertSource>());
builder.Services.AddHostedService<ScraperWorker>();

var host = builder.Build();

using (var scope = host.Services.CreateScope())
{
	var dbContext = scope.ServiceProvider.GetRequiredService<ConcertRadarDbContext>();
	dbContext.Database.Migrate();
}

host.Run();

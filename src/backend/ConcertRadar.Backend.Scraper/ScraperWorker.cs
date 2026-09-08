using ConcertRadar.Backend.Infrastructure.Persistence;
using ConcertRadar.Backend.Scraper.Importing;
using ConcertRadar.Backend.Scraper.Scraping;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace ConcertRadar.Backend.Scraper;

public sealed class ScraperWorker(
	IEnumerable<IConcertSource> sources,
	IServiceScopeFactory scopeFactory,
	IOptions<HtmlSourceOptions> options,
	ILogger<ScraperWorker> logger) : BackgroundService
{
	protected override async Task ExecuteAsync(CancellationToken stoppingToken)
	{
		await RunOnceAsync(stoppingToken);
		using var timer = new PeriodicTimer(options.Value.Interval);

		while (await timer.WaitForNextTickAsync(stoppingToken))
		{
			await RunOnceAsync(stoppingToken);
		}
	}

	private async Task RunOnceAsync(CancellationToken cancellationToken)
	{
		foreach (var source in sources)
		{
			try
			{
				var scrapedEvents = await source.GetEventsAsync(cancellationToken);
				if (scrapedEvents.Count == 0)
				{
					continue;
				}

				using var scope = scopeFactory.CreateScope();
				var importer = scope.ServiceProvider.GetRequiredService<ConcertImporter>();
				await importer.ImportAsync(scrapedEvents, cancellationToken);
				logger.LogInformation("Importados {Count} eventos desde {Source}.", scrapedEvents.Count, source.Name);
			}
			catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
			{
				return;
			}
			catch (Exception exception)
			{
				logger.LogError(exception, "Error importando datos desde {Source}.", source.Name);
			}
		}
	}
}
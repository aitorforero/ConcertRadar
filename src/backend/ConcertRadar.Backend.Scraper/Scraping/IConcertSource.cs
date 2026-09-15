namespace ConcertRadar.Backend.Scraper.Scraping;

public interface IConcertSource
{
	string Name { get; }

	Task<IReadOnlyCollection<ScrapedEvent>> GetEventsAsync(
		CancellationToken cancellationToken);
}
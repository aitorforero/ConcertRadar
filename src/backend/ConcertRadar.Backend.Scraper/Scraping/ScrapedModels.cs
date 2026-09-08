namespace ConcertRadar.Backend.Scraper.Scraping;

public record ScrapedEvent(
	string Source,
	string ExternalId,
	string Name,
	string? Description,
	DateTime StartDate,
	DateTime? EndDate,
	string Venue,
	string? City,
	string? Address,
	string? TicketUrl,
	IReadOnlyCollection<ScrapedPerformance> Performances);

public record ScrapedPerformance(
	string ExternalId,
	string BandExternalId,
	string BandName,
	string GenreExternalId,
	string GenreName,
	DateTime Date);

public interface IConcertSource
{
	string Name { get; }

	Task<IReadOnlyCollection<ScrapedEvent>> GetEventsAsync(
		CancellationToken cancellationToken);
}
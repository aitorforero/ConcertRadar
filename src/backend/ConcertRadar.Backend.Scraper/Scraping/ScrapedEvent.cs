namespace ConcertRadar.Backend.Scraper.Scraping;

public record ScrapedEvent(
	string Source,
	string? ExternalId,
	string Name,
	DateTime StartDate,
	DateTime? EndDate,
	string Venue,
	string? Address,
	string? Province,
	string? City,
	string? Country,
	IReadOnlyCollection<ScrapedPerformance> Performances);

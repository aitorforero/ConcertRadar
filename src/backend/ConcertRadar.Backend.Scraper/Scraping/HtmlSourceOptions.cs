namespace ConcertRadar.Backend.Scraper.Scraping;

public class HtmlSourceOptions
{
	public string Source { get; set; } = string.Empty;

	public string Url { get; set; } = string.Empty;

	public string EventSelector { get; set; } = string.Empty;

	public string EventIdAttribute { get; set; } = "data-event-id";

	public string EventNameSelector { get; set; } = string.Empty;

	public string EventDescriptionSelector { get; set; } = string.Empty;

	public string EventStartDateSelector { get; set; } = string.Empty;

	public string EventEndDateSelector { get; set; } = string.Empty;

	public string EventVenueSelector { get; set; } = string.Empty;

	public string EventCitySelector { get; set; } = string.Empty;

	public string EventAddressSelector { get; set; } = string.Empty;

	public string EventTicketUrlSelector { get; set; } = string.Empty;

	public string PerformanceSelector { get; set; } = string.Empty;

	public string PerformanceIdAttribute { get; set; } = "data-performance-id";

	public string PerformanceDateSelector { get; set; } = string.Empty;

	public string BandNameSelector { get; set; } = string.Empty;

	public string BandIdAttribute { get; set; } = "data-band-id";

	public string GenreSelector { get; set; } = string.Empty;

	public TimeSpan Interval { get; set; } = TimeSpan.FromHours(6);
}
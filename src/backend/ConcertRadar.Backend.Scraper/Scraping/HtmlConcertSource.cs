using AngleSharp;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ConcertRadar.Backend.Scraper.Scraping;

public sealed class HtmlConcertSource(
	HttpClient httpClient,
	IOptions<HtmlSourceOptions> options,
	ILogger<HtmlConcertSource> logger) : IConcertSource
{
	private readonly HtmlSourceOptions settings = options.Value;

	public string Name => settings.Source;

	public async Task<IReadOnlyCollection<ScrapedEvent>> GetEventsAsync(
		CancellationToken cancellationToken)
	{
		if (!Uri.TryCreate(settings.Url, UriKind.Absolute, out var url) ||
			string.IsNullOrWhiteSpace(settings.Source) ||
			string.IsNullOrWhiteSpace(settings.EventSelector))
		{
			logger.LogDebug("Fuente HTML deshabilitada: falta URL, Source o EventSelector.");
			return [];
		}

		var html = await httpClient.GetStringAsync(url, cancellationToken);
		var browsingContext = BrowsingContext.New(Configuration.Default);
		var document = await browsingContext.OpenAsync(request => request.Content(html), cancellationToken);

		return document.QuerySelectorAll(settings.EventSelector)
			.Select(ParseEvent)
			.Where(@event => @event is not null)
			.Cast<ScrapedEvent>()
			.ToArray();
	}

	private ScrapedEvent? ParseEvent(AngleSharp.Dom.IElement element)
	{
		var externalId = ReadAttribute(element, settings.EventIdAttribute) ??
			ReadValue(element, settings.EventNameSelector);
		var name = ReadValue(element, settings.EventNameSelector);
		var startText = ReadValue(element, settings.EventStartDateSelector);

		if (string.IsNullOrWhiteSpace(externalId) || string.IsNullOrWhiteSpace(name) ||
			!DateTime.TryParse(startText, out var startDate))
		{
			logger.LogWarning("Evento ignorado porque falta ID, nombre o fecha: {Name}", name);
			return null;
		}

		var performances = element.QuerySelectorAll(settings.PerformanceSelector)
			.Select(performance => ParsePerformance(externalId, performance))
			.Where(performance => performance is not null)
			.Cast<ScrapedPerformance>()
			.ToArray();

		return new ScrapedEvent(
			settings.Source,
			externalId,
			name,
			ReadOptionalValue(element, settings.EventDescriptionSelector),
			startDate,
			ParseOptionalDate(element, settings.EventEndDateSelector),
			ReadValue(element, settings.EventVenueSelector),
			ReadOptionalValue(element, settings.EventCitySelector),
			ReadOptionalValue(element, settings.EventAddressSelector),
			ReadOptionalAttribute(element, settings.EventTicketUrlSelector, "href"),
			performances);
	}

	private ScrapedPerformance? ParsePerformance(string eventId, AngleSharp.Dom.IElement element)
	{
		var bandName = ReadValue(element, settings.BandNameSelector);
		var dateText = ReadValue(element, settings.PerformanceDateSelector);
		if (string.IsNullOrWhiteSpace(bandName) || !DateTime.TryParse(dateText, out var date))
		{
			return null;
		}

		var bandId = ReadAttribute(element, settings.BandIdAttribute) ?? bandName;
		var genreName = ReadOptionalValue(element, settings.GenreSelector) ?? "Unknown";
		var performanceId = ReadAttribute(element, settings.PerformanceIdAttribute) ??
			$"{eventId}:{bandId}:{date:O}";

		return new ScrapedPerformance(
			performanceId,
			bandId,
			bandName,
			genreName,
			genreName,
			date);
	}

	private static string ReadValue(AngleSharp.Dom.IElement element, string selector) =>
		ReadOptionalValue(element, selector) ?? string.Empty;

	private static string? ReadOptionalValue(AngleSharp.Dom.IElement element, string selector) =>
		string.IsNullOrWhiteSpace(selector) ? null : element.QuerySelector(selector)?.TextContent.Trim();

	private static string? ReadAttribute(AngleSharp.Dom.IElement element, string attribute) =>
		string.IsNullOrWhiteSpace(attribute) ? null : element.GetAttribute(attribute)?.Trim();

	private static string? ReadOptionalAttribute(AngleSharp.Dom.IElement element, string selector, string attribute) =>
		string.IsNullOrWhiteSpace(selector) ? null : element.QuerySelector(selector)?.GetAttribute(attribute)?.Trim();

	private static DateTime? ParseOptionalDate(AngleSharp.Dom.IElement element, string selector) =>
		DateTime.TryParse(ReadOptionalValue(element, selector), out var value) ? value : null;
}
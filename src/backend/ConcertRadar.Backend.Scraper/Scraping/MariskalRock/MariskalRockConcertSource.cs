using AngleSharp;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Text.RegularExpressions;

namespace ConcertRadar.Backend.Scraper.Scraping.MariskalRock;

public sealed class MariskalRockConcertSource(
	HttpClient httpClient,
	IOptions<HtmlSourceOptions> options,
	ILogger<MariskalRockConcertSource> logger) : IConcertSource
{
	public string Name { get; } = "Mariskal Rock";

    private const string pageUrl = "https://mariskalrock.com/guia-de-conciertos/";
	private const string initialRegex = @"<h1>(?><strong>)?(?<inicial>[A-Z#])(?></strong>)?</h1>";

	public async Task<IReadOnlyCollection<ScrapedEvent>> GetEventsAsync(
		CancellationToken cancellationToken)
	{
		if (!Uri.TryCreate(pageUrl, UriKind.Absolute, out var url))
		{
			logger.LogDebug("Fuente HTML deshabilitada: falta URL.");
			return [];
		}

		var html = await httpClient.GetStringAsync(url, cancellationToken);

		foreach (Match match in Regex.Matches(html, initialRegex))
		{
			Console.WriteLine(match.Value);
		}

		return [];
	}
}